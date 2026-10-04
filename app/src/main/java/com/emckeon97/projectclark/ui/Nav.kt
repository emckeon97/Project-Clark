package com.emckeon97.projectclark.ui

import androidx.compose.runtime.Composable
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument

object Routes {
    const val MENU = "menu"
    const val GAME = "game"
    const val GAME_OVER = "gameOver/{score}"
    fun gameOver(score: Int) = "gameOver/$score"
}

@Composable
fun AppNav() {
    val navController = rememberNavController()
    NavHost(navController = navController, startDestination = Routes.MENU) {
        composable(Routes.MENU) { MenuScreen(navController) }
        composable(Routes.GAME) { GameScreen(navController) }
        composable(
            Routes.GAME_OVER,
            arguments = listOf(navArgument("score") { type = NavType.IntType })
        ) { entry ->
            GameOverScreen(navController, entry.arguments?.getInt("score") ?: 0)
        }
    }
}
