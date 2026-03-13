package com.example.systemotaupdater.network

import com.example.systemotaupdater.data.UpdateInfo
import retrofit2.http.GET

/**
 * Retrofit API definition for OTA metadata.
 */
interface UpdateApi {
    @GET("update.json")
    suspend fun getUpdateInfo(): UpdateInfo
}
