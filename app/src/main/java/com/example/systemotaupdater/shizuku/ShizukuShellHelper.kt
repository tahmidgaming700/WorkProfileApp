package com.example.systemotaupdater.shizuku

import android.content.Context
import android.content.pm.PackageManager
import androidx.core.content.ContextCompat
import rikka.shizuku.Shizuku
import java.io.BufferedReader
import java.io.InputStreamReader

/**
 * Utility wrapper around Shizuku process execution.
 *
 * NOTE: `adb sideload` is normally host-side, so on-device fallback uses update_engine_client.
 */
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
        if (!Shizuku.isPreV11()) {
            Shizuku.requestPermission(requestCode)
        }
    }

    /** Executes a shell command through Shizuku and returns command output. */
    fun executeCommand(cmd: String): Result<String> = runCatching {
        val process = Shizuku.newProcess(arrayOf("sh", "-c", cmd), null, null)
        val stdout = BufferedReader(InputStreamReader(process.inputStream)).use { it.readText() }
        val stderr = BufferedReader(InputStreamReader(process.errorStream)).use { it.readText() }
        val exitCode = process.waitFor()
        if (exitCode != 0) {
            error("Command failed ($exitCode): $stderr")
        }
        stdout.ifBlank { "Command completed successfully." }
    }

    /**
     * Tries both host-like sideload syntax and update_engine_client payload application.
     * This supports Shizuku sessions started through USB or wireless ADB.
     */
    fun sideloadUpdate(zipPath: String): Result<String> {
        val escaped = zipPath.replace("\"", "\\\"")
        val adbStyle = executeCommand("adb sideload \"$escaped\"")
        if (adbStyle.isSuccess) {
            return adbStyle
        }

        val payloadStyle = executeCommand(
            "update_engine_client --update --payload=file://$escaped --follow"
        )
        if (payloadStyle.isSuccess) {
            return payloadStyle
        }

        return Result.failure(
            IllegalStateException(
                "Both sideload strategies failed. adb sideload error=" +
                    (adbStyle.exceptionOrNull()?.message ?: "unknown") +
                    "; update_engine_client error=" +
                    (payloadStyle.exceptionOrNull()?.message ?: "unknown")
            )
        )
    }
}
