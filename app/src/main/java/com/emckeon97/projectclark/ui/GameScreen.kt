package com.emckeon97.projectclark.ui

import android.app.Activity
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.NavController
import com.emckeon97.projectclark.Prefs
import com.emckeon97.projectclark.ads.AdManager
import com.emckeon97.projectclark.characters.rememberSpriteBitmap
import com.emckeon97.projectclark.game.FlappyCanvas
import com.emckeon97.projectclark.game.FlappyEngine
import com.emckeon97.projectclark.game.MusicManager

/** The flight itself: tap anywhere to flap. */
@Composable
fun GameScreen(navController: NavController) {
    val context = LocalContext.current
    val engine = remember { FlappyEngine() }
    val sprite = rememberSpriteBitmap(Prefs.selected(context))

    LaunchedEffect(Unit) {
        MusicManager.play(context)
    }

    BoxWithConstraints(
        modifier = Modifier
            .fillMaxSize()
            .background(ClarkTheme.ink)
    ) {
        val wDp = maxWidth.value
        val hDp = maxHeight.value
        // Seed the engine once the layout size is known (portrait is locked).
        LaunchedEffect(wDp, hDp) {
            if (wDp > 0 && engine.screenW == 400f && engine.screenH == 800f) {
                engine.reset(wDp, hDp)
            }
        }
        FlappyCanvas(
            engine = engine,
            sprite = sprite,
            onGameOver = { score ->
                AdManager.gameOverOccurred(context as? Activity)
                navController.navigate(Routes.gameOver(score)) {
                    popUpTo(Routes.MENU)
                }
            },
            modifier = Modifier.fillMaxSize()
        )
    }
}
