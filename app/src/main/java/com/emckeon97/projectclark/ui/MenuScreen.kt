package com.emckeon97.projectclark.ui

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.navigation.NavController
import com.emckeon97.projectclark.Prefs
import com.emckeon97.projectclark.ads.AdManager
import com.emckeon97.projectclark.characters.ROSTER
import com.emckeon97.projectclark.characters.spriteResFor
import com.emckeon97.projectclark.game.MusicManager

/** 1930s movie-poster marquee menu. */
@Composable
fun MenuScreen(navController: NavController) {
    val context = LocalContext.current
    var selectedID by remember { mutableStateOf(Prefs.selected(context)) }
    val best = Prefs.best(context)
    val muted by MusicManager.isMuted.collectAsState()

    LaunchedEffect(Unit) {
        AdManager.loadInterstitial(context)
        MusicManager.play(context)
    }

    Box(modifier = Modifier.fillMaxSize().background(ClarkTheme.ink)) {
        Column(modifier = Modifier.fillMaxSize()) {
            Column(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally,
                verticalArrangement = Arrangement.Center
            ) {
                Text(
                    text = "NOW SHOWING",
                    color = ClarkTheme.cream.copy(alpha = 0.75f),
                    fontSize = 13.sp,
                    fontWeight = FontWeight.SemiBold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 6.sp
                )
                MarqueeLights(modifier = Modifier.padding(vertical = 10.dp))
                Text(
                    text = "RIVER REEL",
                    color = ClarkTheme.cream,
                    fontSize = 46.sp,
                    fontWeight = FontWeight.Black,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 2.sp,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.fillMaxWidth()
                )
                Text(
                    text = "A 1930s FLAPPY CARTOON",
                    color = ClarkTheme.gold,
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 5.sp,
                    modifier = Modifier.padding(top = 8.dp)
                )
                Spacer(Modifier.height(18.dp))
                // character picker
                Text(
                    text = "CHOOSE YOUR FLYER",
                    color = ClarkTheme.cream.copy(alpha = 0.7f),
                    fontSize = 12.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 3.sp
                )
                Spacer(Modifier.height(10.dp))
                LazyRow(
                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                    contentPadding = PaddingValues(horizontal = 8.dp)
                ) {
                    items(ROSTER) { toon ->
                        val res = spriteResFor(toon.id)
                        val isSel = toon.id == selectedID
                        Box(
                            modifier = Modifier
                                .size(72.dp)
                                .clip(CircleShape)
                                .background(ClarkTheme.cream.copy(alpha = 0.06f))
                                .border(
                                    width = if (isSel) 3.dp else 1.dp,
                                    color = if (isSel) ClarkTheme.gold
                                    else ClarkTheme.cream.copy(alpha = 0.25f),
                                    shape = CircleShape
                                )
                                .clickable {
                                    selectedID = toon.id
                                    Prefs.setSelected(context, toon.id)
                                },
                            contentAlignment = Alignment.Center
                        ) {
                            if (res != null) {
                                Image(
                                    painter = painterResource(id = res),
                                    contentDescription = toon.name,
                                    modifier = Modifier.size(60.dp),
                                    contentScale = ContentScale.Fit
                                )
                            }
                        }
                    }
                }
                Spacer(Modifier.height(6.dp))
                Text(
                    text = ROSTER.first { it.id == selectedID }.name.uppercase(),
                    color = ClarkTheme.cream.copy(alpha = 0.9f),
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    letterSpacing = 2.sp
                )
                Spacer(Modifier.height(14.dp))
                // best score pill
                Box(
                    modifier = Modifier
                        .border(
                            1.dp, ClarkTheme.gold.copy(alpha = 0.35f),
                            RoundedCornerShape(14.dp)
                        )
                        .background(
                            ClarkTheme.cream.copy(alpha = 0.07f),
                            RoundedCornerShape(14.dp)
                        )
                        .padding(horizontal = 18.dp, vertical = 8.dp)
                ) {
                    Text(
                        text = "\u2605 BEST $best",
                        color = ClarkTheme.gold,
                        fontSize = 17.sp,
                        fontWeight = FontWeight.Bold,
                        fontFamily = FontFamily.Serif
                    )
                }
                Spacer(Modifier.height(16.dp))
                // ticket-stub PLAY button
                Button(
                    onClick = { navController.navigate(Routes.GAME) },
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(10.dp),
                    colors = ButtonDefaults.buttonColors(
                        containerColor = ClarkTheme.gold,
                        contentColor = ClarkTheme.ink
                    )
                ) {
                    Column(
                        horizontalAlignment = Alignment.CenterHorizontally,
                        modifier = Modifier.padding(horizontal = 54.dp, vertical = 13.dp)
                    ) {
                        Text(
                            text = "\u2605 ADMIT ONE \u2605",
                            fontSize = 11.sp,
                            fontWeight = FontWeight.Bold,
                            fontFamily = FontFamily.Serif,
                            letterSpacing = 3.sp
                        )
                        Text(
                            text = "PLAY",
                            fontSize = 30.sp,
                            fontWeight = FontWeight.Black,
                            fontFamily = FontFamily.Serif,
                            letterSpacing = 5.sp
                        )
                    }
                }
                Spacer(Modifier.height(12.dp))
                Text(
                    text = "Music: \"The Entertainer\" by Kevin MacLeod (incompetech.com) \u00B7 CC BY 4.0",
                    color = ClarkTheme.cream.copy(alpha = 0.35f),
                    fontSize = 9.sp,
                    textAlign = TextAlign.Center,
                    modifier = Modifier.padding(top = 8.dp)
                )
            }
            AndroidView(
                factory = { ctx -> AdManager.bannerView(ctx) },
                modifier = Modifier.fillMaxWidth()
            )
        }
        // mute toggle
        Button(
            onClick = { MusicManager.toggleMute(context) },
            modifier = Modifier
                .align(Alignment.TopEnd)
                .padding(top = 54.dp, end = 16.dp),
            shape = RoundedCornerShape(12.dp),
            colors = ButtonDefaults.buttonColors(
                containerColor = ClarkTheme.cream.copy(alpha = 0.06f),
                contentColor = ClarkTheme.gold
            ),
            contentPadding = PaddingValues(12.dp)
        ) {
            Text(
                text = if (muted) "\uD83D\uDD07" else "\uD83D\uDD0A",
                fontSize = 20.sp
            )
        }
    }
}
