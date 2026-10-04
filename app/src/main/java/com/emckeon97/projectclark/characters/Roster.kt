package com.emckeon97.projectclark.characters

import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.ImageBitmap
import androidx.compose.ui.res.imageResource
import com.emckeon97.projectclark.R

/** The playable toons — the Project Delta sprite roster. */
data class Toon(val id: String, val name: String)

val ROSTER = listOf(
    Toon("popeye", "Popeye"),
    Toon("felix", "Felix the Cat"),
    Toon("oswald", "Oswald"),
    Toon("koko", "Koko"),
    Toon("bimbo", "Bimbo"),
    Toon("pooh", "Winnie the Pooh"),
    Toon("olive", "Olive Oyl"),
    Toon("bosko", "Bosko"),
    Toon("pete", "Peg-Leg Pete"),
)

fun spriteResFor(id: String): Int? = when (id) {
    "felix" -> R.drawable.felix
    "popeye" -> R.drawable.popeye
    "oswald" -> R.drawable.oswald
    "koko" -> R.drawable.koko
    "bimbo" -> R.drawable.bimbo
    "pooh" -> R.drawable.pooh
    "olive" -> R.drawable.olive
    "bosko" -> R.drawable.bosko
    "pete" -> R.drawable.pete
    else -> null
}

/** Loads the sprite bitmap for [id], or null when the art isn't bundled. */
@Composable
fun rememberSpriteBitmap(id: String): ImageBitmap? {
    val res = spriteResFor(id) ?: return null
    return ImageBitmap.imageResource(res)
}
