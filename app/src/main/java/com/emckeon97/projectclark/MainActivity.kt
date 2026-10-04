package com.emckeon97.projectclark

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.LifecycleEventObserver
import com.emckeon97.projectclark.ads.AdManager
import com.emckeon97.projectclark.game.MusicManager
import com.emckeon97.projectclark.ui.AppNav

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        enableEdgeToEdge()
        super.onCreate(savedInstanceState)
        // Never play audio when the app isn't actively being used.
        lifecycle.addObserver(LifecycleEventObserver { _, event ->
            when (event) {
                Lifecycle.Event.ON_PAUSE -> MusicManager.pause()
                Lifecycle.Event.ON_RESUME -> MusicManager.play(this)
                else -> {}
            }
        })
        AdManager.initialize(this)
        setContent {
            MaterialTheme(colorScheme = darkColorScheme()) {
                AppNav()
            }
        }
    }
}
