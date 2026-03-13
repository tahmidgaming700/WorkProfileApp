package com.example.systemotaupdater.data

import com.google.gson.annotations.SerializedName

/**
 * Update metadata returned by https://yourserver.com/update.json
 */
data class UpdateInfo(
    @SerializedName("version") val version: String,
    @SerializedName("url") val url: String,
    @SerializedName("sha256") val sha256: String,
    @SerializedName("notes") val notes: String
)
