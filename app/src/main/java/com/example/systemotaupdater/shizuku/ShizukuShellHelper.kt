package com.example.systemotaupdater.shizuku

import android.content.Context
import android.content.pm.PackageManager
import androidx.core.content.ContextCompat
import rikka.shizuku.Shizuku
import java.io.BufferedReader
import java.io.InputStreamReader

class ShizukuShellHelper {

    fun isShizukuAvailable(): Boolean = Shizuku.pingBinder()

    fun hasShizukuPermission(context: Context): Boolean {
        return if (Shizuku.isPreV11()) {
            ContextCompat.checkSelfPermission(
                context,
                "moe.shizuku.manager.permission.API_V23"
            ) == PackageManager.PERMISSION_GRANTED
        } else {
            Shizuku.checkSelfPermission() == PackageManager.PERMISSION_GRANTED
        }
    }

    fun requestPermission(requestCode: Int) {
        if (Shizuku.isPreV11()) return
        Shizuku.requestPermission(requestCode)
    }

    fun runCommand(command: String): Result<String> {
        return runCatching {
            val process = Shizuku.newProcess(arrayOf("sh", "-c", command), null, null)
            val output = BufferedReader(InputStreamReader(process.inputStream)).use { it.readText() }
            val error = BufferedReader(InputStreamReader(process.errorStream)).use { it.readText() }
            val exitCode = process.waitFor()
            if (exitCode != 0) {
                error("Command failed ($exitCode): $error")
            }
            output
        }
    }

    fun sideloadDownloadedZip(zipPath: String): Result<String> {
        val command = "adb sideload \"$zipPath\""
        return runCommand(command)
    }
}
