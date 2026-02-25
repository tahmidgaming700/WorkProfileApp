package com.example.systemotaupdater.data

data class UpdateInfo(
    val latestVersion: String,
    val incrementalUrl: String,
    val fullUrl: String,
    val releaseNotes: String,
    val checksum: String
)
