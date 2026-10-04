package com.emckeon97.projectclark.game

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableLongStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.runtime.withFrameNanos
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ImageBitmap
import androidx.compose.ui.graphics.drawscope.withTransform
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.drawText
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.rememberTextMeasurer
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.sp
import com.emckeon97.projectclark.ui.ClarkTheme
import kotlin.math.abs
import kotlin.math.exp
import kotlin.math.sin

/**
 * Night-flight rendering: moonlit sky, drifting riverboat, riveted smokestack
 * pairs, the flapping toon, pier deck, score, and get-ready prompt.
 */
@Composable
fun FlappyCanvas(
    engine: FlappyEngine,
    sprite: ImageBitmap?,
    onGameOver: (Int) -> Unit,
    modifier: Modifier = Modifier
) {
    val density = LocalDensity.current
    val textMeasurer = rememberTextMeasurer()
    var tick by remember { mutableLongStateOf(0L) }
    var overFired by remember { mutableStateOf(false) }

    LaunchedEffect(Unit) {
        var last = 0L
        while (true) {
            withFrameNanos { now ->
                if (last == 0L) last = now
                val dtMs = ((now - last) / 1_000_000).coerceAtMost(50)
                last = now
                engine.update(dtMs / 1000f)
                if (engine.gameOver && engine.deadT > 0.8f && !overFired) {
                    overFired = true
                    onGameOver(engine.score)
                }
                tick = now
            }
        }
    }

    Canvas(
        modifier.pointerInput(Unit) { detectTapGestures { engine.flap() } }
    ) {
        val tSec = tick / 1_000_000_000f
        val d = density.density
        fun px(dp: Float) = dp * d
        val w = size.width
        val h = size.height
        val groundYPx = px(engine.groundY)

        // ---- night sky ----
        drawRect(
            Brush.verticalGradient(
                listOf(Color(0xFF05070F), Color(0xFF0D1330)),
                endY = groundYPx
            ),
            size = Size(w, groundYPx)
        )
        // ---- stars ----
        val starRand = kotlin.random.Random(7)
        repeat(42) {
            val sx = starRand.nextFloat() * w
            val sy = starRand.nextFloat() * groundYPx * 0.85f
            val sr = (0.8f + starRand.nextFloat() * 1.4f) * d
            val tw = 0.35f + 0.65f * abs(sin(tSec * (1f + starRand.nextFloat() * 2f) + it))
            drawCircle(Color.White.copy(alpha = 0.7f * tw), sr, Offset(sx, sy))
        }
        // ---- moon + halo ----
        val moonX = w * 0.80f
        val moonY = h * 0.13f
        val moonR = h * 0.042f
        drawCircle(Color(0xFFF5EFE0).copy(alpha = 0.10f), moonR * 2.6f, Offset(moonX, moonY))
        drawCircle(Color(0xFFF5EFE0).copy(alpha = 0.16f), moonR * 1.7f, Offset(moonX, moonY))
        drawCircle(Color(0xFFF2EBD8), moonR, Offset(moonX, moonY))

        // ---- distant riverboat silhouette, drifting ----
        val boatW = px(150f)
        val boatX = w - ((tSec * px(14f)) % (w + boatW * 2)) + boatW
        val boatY = groundYPx - px(46f)
        drawRoundRect(
            Color(0xFF0B0E18), Offset(boatX, boatY),
            Size(boatW, px(26f)), CornerRadius(px(6f), px(6f))
        )
        drawRect(Color(0xFF0B0E18), Offset(boatX + boatW * 0.3f, boatY - px(20f)), Size(boatW * 0.4f, px(20f)))
        // lit windows
        for (i in 0..3) {
            drawCircle(
                Color(0xFFD4A942).copy(alpha = 0.75f),
                px(3f),
                Offset(boatX + boatW * (0.18f + i * 0.21f), boatY + px(13f))
            )
        }

        // ---- smokestack pairs ----
        for (s in engine.stacks) {
            val sx = px(s.x)
            val sw = px(FlappyEngine.PIPE_W)
            val gapTop = px(s.gapY - s.gap / 2f)
            val gapBot = px(s.gapY + s.gap / 2f)
            drawStack(sx, sw, 0f, gapTop, d, isTop = true)
            drawStack(sx, sw, gapBot, groundYPx, d, isTop = false)
            // lantern glow marking the gap
            drawCircle(
                Color(0xFFD4A942).copy(alpha = 0.28f), px(16f),
                Offset(sx + sw / 2f, gapTop - px(4f))
            )
            drawCircle(
                Color(0xFFD4A942).copy(alpha = 0.28f), px(16f),
                Offset(sx + sw / 2f, gapBot + px(4f))
            )
            drawCircle(Color(0xFFE8C25A), px(5f), Offset(sx + sw / 2f, gapTop - px(4f)))
            drawCircle(Color(0xFFE8C25A), px(5f), Offset(sx + sw / 2f, gapBot + px(4f)))
        }

        // ---- pier deck (the ground) ----
        drawRect(Color(0xFF241C12), Offset(0f, groundYPx), Size(w, h - groundYPx))
        drawRect(Color(0xFF2E2517), Offset(0f, groundYPx), Size(w, px(10f)))
        drawLine(
            Color(0xFFD4A942).copy(alpha = 0.5f),
            Offset(0f, groundYPx + px(1f)), Offset(w, groundYPx + px(1f)),
            strokeWidth = px(2f)
        )
        // plank seams scrolling with the world
        val seamMod = (engine.stacks.firstOrNull()?.x ?: 0f) % 48f
        var sxp = -seamMod * d
        while (sxp < w) {
            drawLine(
                Color.Black.copy(alpha = 0.35f),
                Offset(sxp, groundYPx + px(10f)), Offset(sxp, h),
                strokeWidth = px(2f)
            )
            sxp += px(48f)
        }

        // ---- the toon ----
        val bx = px(engine.birdX)
        val by = px(engine.birdY)
        val birdPx = px(64f)
        val pulse = 1f + 0.13f * exp(-engine.flapT * 7f).toFloat()
        if (sprite != null) {
            val iw = sprite.width.toFloat()
            val ih = sprite.height.toFloat()
            val dst = Size(birdPx, birdPx * ih / iw)
            withTransform({
                rotate(engine.rotation, pivot = Offset(bx, by))
                scale(pulse, pulse, pivot = Offset(bx, by))
            }) {
                drawImage(
                    image = sprite,
                    dstOffset = androidx.compose.ui.unit.IntOffset(
                        (bx - dst.width / 2f).toInt(), (by - dst.height / 2f).toInt()
                    ),
                    dstSize = androidx.compose.ui.unit.IntSize(dst.width.toInt(), dst.height.toInt())
                )
            }
        } else {
            // fallback round bird
            withTransform({ rotate(engine.rotation, pivot = Offset(bx, by)) }) {
                drawCircle(Color(0xFFF2EBD8), birdPx * 0.42f, Offset(bx, by))
                drawCircle(Color.Black, px(4f), Offset(bx + px(8f), by - px(6f)))
                drawRoundRect(
                    Color(0xFFD4A942),
                    Offset(bx + birdPx * 0.30f, by - px(3f)),
                    Size(px(12f), px(7f)), CornerRadius(px(3f), px(3f))
                )
            }
        }

        // ---- score ----
        drawText(
            textMeasurer = textMeasurer,
            text = "${engine.score}",
            topLeft = Offset(0f, px(64f)),
            size = Size(w, px(90f)),
            style = TextStyle(
                color = ClarkTheme.cream,
                fontSize = 64.sp,
                fontWeight = FontWeight.Black,
                fontFamily = FontFamily.Serif,
                textAlign = TextAlign.Center,
                shadow = androidx.compose.ui.graphics.Shadow(
                    color = Color.Black.copy(alpha = 0.6f),
                    offset = Offset(px(3f), px(3f)),
                    blurRadius = px(4f)
                )
            )
        )

        // ---- get ready ----
        if (!engine.started && !engine.gameOver) {
            val blink = 0.65f + 0.35f * sin(tSec * 5f)
            drawText(
                textMeasurer = textMeasurer,
                text = "GET READY!",
                topLeft = Offset(0f, h * 0.30f),
                size = Size(w, px(60f)),
                style = TextStyle(
                    color = ClarkTheme.gold.copy(alpha = blink),
                    fontSize = 30.sp,
                    fontWeight = FontWeight.Black,
                    fontFamily = FontFamily.Serif,
                    textAlign = TextAlign.Center,
                    letterSpacing = 3.sp
                )
            )
            drawText(
                textMeasurer = textMeasurer,
                text = "TAP TO FLAP",
                topLeft = Offset(0f, h * 0.30f + px(52f)),
                size = Size(w, px(50f)),
                style = TextStyle(
                    color = ClarkTheme.cream.copy(alpha = 0.85f * blink),
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold,
                    fontFamily = FontFamily.Serif,
                    textAlign = TextAlign.Center,
                    letterSpacing = 2.sp
                )
            )
        }

        // ---- film vignette ----
        drawRect(
            Brush.radialGradient(
                listOf(Color.Transparent, Color.Black.copy(alpha = 0.38f)),
                center = Offset(w / 2f, h / 2f),
                radius = maxOf(w, h) * 0.62f
            )
        )
    }
}

/** One riveted iron smokestack slab, with a cap lip on the gap end. */
private fun androidx.compose.ui.graphics.drawscope.DrawScope.drawStack(
    x: Float, w: Float, top: Float, bottom: Float, d: Float, isTop: Boolean
) {
    if (bottom <= top) return
    val iron = Color(0xFF1A1D24)
    val ironHi = Color(0xFF2E3440)
    drawRect(iron, Offset(x, top), Size(w, bottom - top))
    // vertical highlight
    drawRect(ironHi.copy(alpha = 0.8f), Offset(x + w * 0.14f, top), Size(w * 0.12f, bottom - top))
    // gold bands + rivets
    var y = top + 26f * d
    val bandH = 7f * d
    while (y < bottom - 10f * d) {
        drawRect(Color(0xFFD4A942).copy(alpha = 0.85f), Offset(x, y), Size(w, bandH))
        var rx = x + 10f * d
        while (rx < x + w - 6f * d) {
            drawCircle(Color(0xFF0B0D12), 2.6f * d, Offset(rx, y + bandH / 2f))
            rx += 18f * d
        }
        y += 64f * d
    }
    // cap lip on the gap end
    val lipH = 15f * d
    val lipY = if (isTop) bottom - lipH else top
    drawRect(Color(0xFF0B0D12), Offset(x - 7f * d, lipY), Size(w + 14f * d, lipH))
    drawRect(
        Color(0xFFD4A942).copy(alpha = 0.7f),
        Offset(x - 7f * d, if (isTop) lipY else lipY + lipH - 3f * d),
        Size(w + 14f * d, 3f * d)
    )
}
