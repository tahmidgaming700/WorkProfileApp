package com.example.systemotaupdater.ui

import android.app.AlertDialog
import android.app.DownloadManager
import android.content.Context
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.os.Environment
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

class MainActivity : AppCompatActivity() {

    private val shizukuHelper = ShizukuShellHelper()
    private lateinit var currentVersionText: TextView
    private lateinit var updateInfoText: TextView
    private lateinit var progressBar: ProgressBar
    private lateinit var checkButton: Button

    private var latestUpdateInfo: UpdateInfo? = null
    private var currentDownloadId: Long = -1L

    private val shizukuPermissionListener = Shizuku.OnRequestPermissionResultListener { requestCode, grantResult ->
        if (requestCode == SHIZUKU_REQ_CODE) {
            Toast.makeText(
                this,
                if (grantResult == android.content.pm.PackageManager.PERMISSION_GRANTED) {
                    "Shizuku permission granted"
                } else {
                    "Shizuku permission denied"
                },
                Toast.LENGTH_SHORT
            ).show()
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        currentVersionText = findViewById(R.id.currentVersionText)
        updateInfoText = findViewById(R.id.updateInfoText)
        progressBar = findViewById(R.id.downloadProgressBar)
        checkButton = findViewById(R.id.checkUpdateButton)

        currentVersionText.text = "Current Build: ${Build.DISPLAY} (${Build.VERSION.INCREMENTAL})"
        updateInfoText.text = "Tap \"Check OTA Update\" to fetch update metadata."

        Shizuku.addRequestPermissionResultListener(shizukuPermissionListener)

        checkButton.setOnClickListener {
            fetchAndDisplayUpdate()
        }

        findViewById<Button>(R.id.requestShizukuButton).setOnClickListener {
            ensureShizukuPermission()
        }

        findViewById<Button>(R.id.sideloadButton).setOnClickListener {
            runSideload()
        }
    }

    override fun onDestroy() {
        Shizuku.removeRequestPermissionResultListener(shizukuPermissionListener)
        super.onDestroy()
    }

    private fun fetchAndDisplayUpdate() {
        lifecycleScope.launch {
            updateInfoText.text = "Checking https://yourserver.com/update.json ..."
            val result = runCatching { RetrofitClient.updateApi.getUpdateInfo() }
            result.onSuccess { info ->
                latestUpdateInfo = info
                val validIncremental = isValidUrl(info.incrementalUrl)
                val validFull = isValidUrl(info.fullUrl)
                updateInfoText.text = buildString {
                    appendLine("Latest Version: ${info.latestVersion}")
                    appendLine("Checksum: ${info.checksum}")
                    appendLine("Incremental URL valid: $validIncremental")
                    appendLine("Full URL valid: $validFull")
                    appendLine()
                    appendLine("Release Notes:")
                    appendLine(info.releaseNotes)
                }

                val hasUpdate = info.latestVersion != Build.VERSION.INCREMENTAL
                if (hasUpdate) {
                    showUpdateDialog(info)
                } else {
                    Toast.makeText(this@MainActivity, "You are already on latest build", Toast.LENGTH_SHORT)
                        .show()
                }
            }.onFailure { error ->
                updateInfoText.text = "Failed to fetch update metadata: ${error.message}"
            }
        }
    }

    private fun showUpdateDialog(info: UpdateInfo) {
        AlertDialog.Builder(this)
            .setTitle("System OTA Update Available")
            .setMessage(
                "Version: ${info.latestVersion}\n\nRelease Notes:\n${info.releaseNotes}\n\n" +
                    "Do you want to download the incremental OTA package now?"
            )
            .setPositiveButton("Download") { _, _ ->
                val url = if (isValidUrl(info.incrementalUrl)) info.incrementalUrl else info.fullUrl
                enqueueDownload(url)
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun enqueueDownload(downloadUrl: String) {
        val request = DownloadManager.Request(Uri.parse(downloadUrl))
            .setTitle("OTA Package")
            .setDescription("Downloading OTA update package")
            .setAllowedOverMetered(true)
            .setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)
            .setDestinationInExternalPublicDir(
                Environment.DIRECTORY_DOWNLOADS,
                "ota_update.zip"
            )

        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        currentDownloadId = downloadManager.enqueue(request)

        lifecycleScope.launch { monitorDownload(downloadManager, currentDownloadId) }
    }

    private suspend fun monitorDownload(downloadManager: DownloadManager, downloadId: Long) {
        progressBar.progress = 0
        progressBar.isIndeterminate = false

        while (true) {
            val query = DownloadManager.Query().setFilterById(downloadId)
            val cursor = downloadManager.query(query)
            if (cursor != null && cursor.moveToFirst()) {
                val bytesDownloaded = cursor.getLong(
                    cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_BYTES_DOWNLOADED_SO_FAR)
                )
                val bytesTotal = cursor.getLong(
                    cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_TOTAL_SIZE_BYTES)
                )
                val status = cursor.getInt(
                    cursor.getColumnIndexOrThrow(DownloadManager.COLUMN_STATUS)
                )

                if (bytesTotal > 0) {
                    val progress = ((bytesDownloaded * 100L) / bytesTotal).toInt()
                    progressBar.progress = progress
                }

                if (status == DownloadManager.STATUS_SUCCESSFUL) {
                    cursor.close()
                    Toast.makeText(this, "OTA ZIP download complete", Toast.LENGTH_SHORT).show()
                    break
                } else if (status == DownloadManager.STATUS_FAILED) {
                    cursor.close()
                    Toast.makeText(this, "OTA ZIP download failed", Toast.LENGTH_SHORT).show()
                    break
                }
            }
            cursor?.close()
            delay(1000)
        }
    }

    private fun runSideload() {
        if (currentDownloadId == -1L) {
            Toast.makeText(this, "Download OTA ZIP first", Toast.LENGTH_SHORT).show()
            return
        }
        if (!shizukuHelper.isShizukuAvailable()) {
            Toast.makeText(this, "Shizuku is not running. Enable via wireless/USB debugging.", Toast.LENGTH_LONG).show()
            return
        }
        if (!shizukuHelper.hasShizukuPermission(this)) {
            ensureShizukuPermission()
            return
        }

        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        val fileUri = downloadManager.getUriForDownloadedFile(currentDownloadId)
        if (fileUri == null) {
            Toast.makeText(this, "Downloaded OTA file URI unavailable", Toast.LENGTH_SHORT).show()
            return
        }

        lifecycleScope.launch {
            val result = withContext(Dispatchers.IO) {
                shizukuHelper.sideloadDownloadedZip(fileUri.path.orEmpty())
            }
            Toast.makeText(
                this@MainActivity,
                result.getOrElse { "Sideload failed: ${it.message}" },
                Toast.LENGTH_LONG
            ).show()
        }
    }

    private fun ensureShizukuPermission() {
        when {
            !shizukuHelper.isShizukuAvailable() -> {
                Toast.makeText(
                    this,
                    "Start Shizuku first using USB or wireless debugging.",
                    Toast.LENGTH_LONG
                ).show()
            }

            shizukuHelper.hasShizukuPermission(this) -> {
                Toast.makeText(this, "Shizuku permission already granted", Toast.LENGTH_SHORT).show()
            }

            else -> shizukuHelper.requestPermission(SHIZUKU_REQ_CODE)
        }
    }

    private fun isValidUrl(value: String): Boolean {
        val uri = Uri.parse(value)
        return (uri.scheme == "http" || uri.scheme == "https") && !uri.host.isNullOrBlank()
    }

    companion object {
        private const val SHIZUKU_REQ_CODE = 707
    }
}
