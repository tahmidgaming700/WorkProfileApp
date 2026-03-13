package com.example.systemotaupdater.ui

import android.app.AlertDialog
import android.app.DownloadManager
import android.content.Context
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import com.example.systemotaupdater.R
import com.example.systemotaupdater.data.UpdateInfo
import com.example.systemotaupdater.network.RetrofitClient
import com.example.systemotaupdater.shizuku.ShizukuShellHelper
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import rikka.shizuku.Shizuku
import java.io.File
import java.io.FileInputStream
import java.security.MessageDigest

class MainActivity : AppCompatActivity() {

    private val shizukuHelper = ShizukuShellHelper()

    private lateinit var currentVersionText: TextView
    private lateinit var updateInfoText: TextView
    private lateinit var statusText: TextView
    private lateinit var progressBar: ProgressBar

    private var latestUpdateInfo: UpdateInfo? = null
    private var downloadedZipPath: String? = null
    private var currentDownloadId: Long = -1L

    private val shizukuPermissionListener =
        Shizuku.OnRequestPermissionResultListener { requestCode, grantResult ->
            if (requestCode == SHIZUKU_REQ_CODE) {
                val granted = grantResult == PackageManager.PERMISSION_GRANTED
                Toast.makeText(
                    this,
                    if (granted) "Shizuku permission granted" else "Shizuku permission denied",
                    Toast.LENGTH_SHORT
                ).show()
            }
        }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        currentVersionText = findViewById(R.id.currentVersionText)
        updateInfoText = findViewById(R.id.updateInfoText)
        statusText = findViewById(R.id.statusText)
        progressBar = findViewById(R.id.downloadProgressBar)

        currentVersionText.text = "Current Build.DISPLAY: ${Build.DISPLAY}"
        updateInfoText.text = "Tap \"Check OTA Update\" to fetch metadata from update.json"
        statusText.text = "Idle"

        Shizuku.addRequestPermissionResultListener(shizukuPermissionListener)

        findViewById<Button>(R.id.checkUpdateButton).setOnClickListener { fetchAndDisplayUpdate() }
        findViewById<Button>(R.id.requestShizukuButton).setOnClickListener { ensureShizukuPermission() }
        findViewById<Button>(R.id.sideloadButton).setOnClickListener { runSideload() }
    }

    override fun onDestroy() {
        Shizuku.removeRequestPermissionResultListener(shizukuPermissionListener)
        super.onDestroy()
    }

    /** Fetch OTA metadata and compare server version with local Build.DISPLAY. */
    private fun fetchAndDisplayUpdate() {
        lifecycleScope.launch {
            statusText.text = "Checking update server..."
            val result = runCatching { RetrofitClient.updateApi.getUpdateInfo() }

            result.onSuccess { info ->
                latestUpdateInfo = info
                updateInfoText.text = buildString {
                    appendLine("Server Version: ${info.version}")
                    appendLine("Package URL: ${info.url}")
                    appendLine("SHA-256: ${info.sha256}")
                    appendLine()
                    appendLine("Release Notes:")
                    appendLine(info.notes)
                }

                val updateAvailable = info.version != Build.DISPLAY
                if (updateAvailable) {
                    statusText.text = "Update available"
                    showUpdateDialog(info)
                } else {
                    statusText.text = "Device is up to date"
                    Toast.makeText(this@MainActivity, "You are already on latest build", Toast.LENGTH_SHORT).show()
                }
            }.onFailure { error ->
                statusText.text = "Network error"
                updateInfoText.text = "Failed to fetch metadata: ${error.message}"
                Toast.makeText(this@MainActivity, "Update check failed", Toast.LENGTH_SHORT).show()
            }
        }
    }

    /** Display notes and ask user to begin download. */
    private fun showUpdateDialog(info: UpdateInfo) {
        AlertDialog.Builder(this)
            .setTitle("OTA Update Available")
            .setMessage("Version: ${info.version}\n\nRelease notes:\n${info.notes}")
            .setPositiveButton("Download") { _, _ -> enqueueDownload(info.url) }
            .setNegativeButton("Cancel", null)
            .show()
    }

    /** Download OTA ZIP into app-specific external files directory. */
    private fun enqueueDownload(downloadUrl: String) {
        val outputFile = File(getExternalFilesDir(null), "ota_update_${System.currentTimeMillis()}.zip")
        val request = DownloadManager.Request(Uri.parse(downloadUrl))
            .setTitle("OTA Update Package")
            .setDescription("Downloading full OTA ZIP")
            .setAllowedOverMetered(true)
            .setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)
            .setDestinationUri(Uri.fromFile(outputFile))

        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        currentDownloadId = downloadManager.enqueue(request)
        downloadedZipPath = outputFile.absolutePath

        lifecycleScope.launch {
            statusText.text = "Downloading OTA..."
            monitorDownload(downloadManager, currentDownloadId)
        }
    }

    /** Poll DownloadManager, update progress bar, and verify SHA-256 after completion. */
    private suspend fun monitorDownload(downloadManager: DownloadManager, downloadId: Long) {
        progressBar.progress = 0

        while (true) {
            val query = DownloadManager.Query().setFilterById(downloadId)
            downloadManager.query(query).use { cursor ->
                if (cursor != null && cursor.moveToFirst()) {
                    val downloaded = cursor.getLong(cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_BYTES_DOWNLOADED_SO_FAR))
                    val total = cursor.getLong(cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_TOTAL_SIZE_BYTES))
                    val status = cursor.getInt(cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_STATUS))

                    if (total > 0L) {
                        progressBar.progress = ((downloaded * 100L) / total).toInt()
                    }

                    when (status) {
                        DownloadManager.STATUS_SUCCESSFUL -> {
                            statusText.text = "Download complete. Verifying SHA-256..."
                            val filePath = resolveDownloadedPath(cursor, downloadId)
                            val updateInfo = latestUpdateInfo

                            if (filePath.isNullOrBlank() || updateInfo == null) {
                                statusText.text = "File resolution failed"
                                Toast.makeText(this, "Cannot resolve downloaded file", Toast.LENGTH_SHORT).show()
                                return
                            }

                            val checksumValid = withContext(Dispatchers.IO) {
                                verifySha256(filePath, updateInfo.sha256)
                            }

                            if (checksumValid) {
                                downloadedZipPath = filePath
                                statusText.text = "Checksum valid. Ready to sideload."
                                Toast.makeText(this, "Checksum verified", Toast.LENGTH_SHORT).show()
                            } else {
                                statusText.text = "Checksum mismatch"
                                Toast.makeText(this, "SHA-256 verification failed", Toast.LENGTH_LONG).show()
                            }
                            return
                        }

                        DownloadManager.STATUS_FAILED -> {
                            statusText.text = "Download failed"
                            Toast.makeText(this, "OTA ZIP download failed", Toast.LENGTH_LONG).show()
                            return
                        }
                    }
                }
            }
            delay(500)
        }
    }

    /** Execute sideload through Shizuku after runtime permission check. */
    private fun runSideload() {
        val zipPath = downloadedZipPath
        if (zipPath.isNullOrBlank()) {
            Toast.makeText(this, "Download OTA ZIP first", Toast.LENGTH_SHORT).show()
            return
        }

        if (!shizukuHelper.isShizukuAvailable()) {
            Toast.makeText(
                this,
                "Shizuku is not running. Start with wireless ADB or USB ADB first.",
                Toast.LENGTH_LONG
            ).show()
            return
        }

        if (!shizukuHelper.hasShizukuPermission(this)) {
            ensureShizukuPermission()
            return
        }

        lifecycleScope.launch {
            statusText.text = "Executing sideload command..."
            val result = withContext(Dispatchers.IO) { shizukuHelper.sideloadUpdate(zipPath) }

            result.onSuccess {
                statusText.text = "Sideload command finished"
                Toast.makeText(this@MainActivity, it, Toast.LENGTH_LONG).show()
            }.onFailure {
                statusText.text = "Sideload failed"
                Toast.makeText(this@MainActivity, "Sideload failed: ${it.message}", Toast.LENGTH_LONG).show()
            }
        }
    }

    private fun ensureShizukuPermission() {
        when {
            !shizukuHelper.isShizukuAvailable() -> {
                Toast.makeText(this, "Start Shizuku using USB ADB or wireless debugging.", Toast.LENGTH_LONG).show()
            }

            shizukuHelper.hasShizukuPermission(this) -> {
                Toast.makeText(this, "Shizuku permission already granted", Toast.LENGTH_SHORT).show()
            }

            else -> shizukuHelper.requestPermission(SHIZUKU_REQ_CODE)
        }
    }

    private fun resolveDownloadedPath(cursor: android.database.Cursor, downloadId: Long): String? {
        val localUriIndex = cursor.getColumnIndex(DownloadManager.COLUMN_LOCAL_URI)
        if (localUriIndex >= 0) {
            val localUriValue = cursor.getString(localUriIndex)
            val localPath = Uri.parse(localUriValue).path
            if (!localPath.isNullOrBlank()) {
                return localPath
            }
        }

        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        val fileUri = downloadManager.getUriForDownloadedFile(downloadId)
        return fileUri?.path
    }

    private fun verifySha256(filePath: String, expectedSha256: String): Boolean {
        val digest = MessageDigest.getInstance("SHA-256")
        FileInputStream(filePath).use { input ->
            val buffer = ByteArray(DEFAULT_BUFFER_SIZE)
            var read = input.read(buffer)
            while (read > 0) {
                digest.update(buffer, 0, read)
                read = input.read(buffer)
            }
        }

        val fileHash = digest.digest().joinToString("") { "%02x".format(it) }
        return fileHash.equals(expectedSha256.trim(), ignoreCase = true)
    }

    companion object {
        private const val SHIZUKU_REQ_CODE = 7011
    }
}
