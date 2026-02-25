package com.example.systemotaupdater.network

import com.example.systemotaupdater.data.UpdateInfo
import retrofit2.http.GET

interface UpdateApiService {
    @GET("update.json")
    suspend fun getUpdateInfo(): UpdateInfo
}
