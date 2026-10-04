package com.emckeon97.projectclark.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.emckeon97.projectclark.Prefs

/** Silent-film "THE END" card with score, best, and retry. */
@Composable
fun GameOverScreen(navController: NavController, score: Int) {
    val context = LocalContext.current
    val prevBest = Prefs.best(context)
    val isRecord = score > prevBest

    LaunchedEffect(Unit) {
        if (isRecord) Prefs.setBest(context, score)
    }
    val best = if (isRecord) score else prevBest

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(ClarkTheme.ink),
        contentAlignment = Alignment.Center
    ) {
        Column(
            modifier = Modifier
                .padding(horizontal = 40.dp)
                .border(2.dp, ClarkTheme.gold, RoundedCornerShape(4.dp))
                .background(ClarkTheme.ink)
                .padding(horizontal = 28.dp, vertical = 28.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            MarqueeLights(count = 14)
            Spacer(Modifier.height(12.dp))
            Text(
                text = "THE END",
                color = ClarkTheme.cream,
                fontSize = 40.sp,
                fontWeight = FontWeight.Black,
                fontFamily = FontFamily.Serif,
                letterSpacing = 4.sp,
                textAlign = TextAlign.Center
            )
            if (isRecord && score > 0) {
                Text(
                    text = "\u2605 NEW REEL RECORD! \u2605",
                    color = ClarkTheme.gold,
                    fontSize = 15.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 2.sp,
                    modifier = Modifier.padding(top = 8.dp)
                )
            }
            Spacer(Modifier.height(18.dp))
            Text(
                text = "SCORE",
                color = ClarkTheme.cream.copy(alpha = 0.6f),
                fontSize = 13.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif,
                letterSpacing = 4.sp
            )
            Text(
                text = "$score",
                color = ClarkTheme.gold,
                fontSize = 64.sp,
                fontWeight = FontWeight.Black,
                fontFamily = FontFamily.Serif
            )
            Text(
                text = "BEST $best",
                color = ClarkTheme.cream.copy(alpha = 0.75f),
                fontSize = 17.sp,
                fontWeight = FontWeight.Bold,
                fontFamily = FontFamily.Serif,
                letterSpacing = 2.sp
            )
            Spacer(Modifier.height(24.dp))
            Button(
                onClick = {
                    navController.navigate(Routes.GAME) {
                        popUpTo(Routes.MENU)
                    }
                },
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(10.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = ClarkTheme.gold,
                    contentColor = ClarkTheme.ink
                )
            ) {
                Text(
                    text = "FLY AGAIN",
                    fontSize = 22.sp,
                    fontWeight = FontWeight.Black,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 3.sp,
                    modifier = Modifier.padding(vertical = 10.dp)
                )
            }
            Spacer(Modifier.height(10.dp))
            OutlinedButton(
                onClick = {
                    navController.navigate(Routes.MENU) {
                        popUpTo(Routes.MENU) { inclusive = true }
                    }
                },
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(10.dp),
                border = androidx.compose.foundation.BorderStroke(
                    2.dp, ClarkTheme.cream.copy(alpha = 0.5f)
                ),
                colors = ButtonDefaults.outlinedButtonColors(
                    contentColor = ClarkTheme.cream
                )
            ) {
                Text(
                    text = "LOBBY",
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 2.sp,
                    modifier = Modifier.padding(vertical = 8.dp)
                )
            }
        }
    }
}
