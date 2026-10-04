package com.emckeon97.projectclark.game

import kotlin.math.min
import kotlin.random.Random

/**
 * Flappy-style flight through the smokestacks. All units are dp; the canvas
 * converts to px. Tap to flap, thread the gaps, don't kiss the pier.
 */
class FlappyEngine {

    data class Stack(val x: Float, val gapY: Float, val gap: Float, var scored: Boolean = false)

    var screenW = 400f
    var screenH = 800f

    var birdX = 120f
    var birdY = 400f
    var vy = 0f
    var rotation = 0f          // degrees, nose-up negative
    var flapT = 99f            // seconds since last flap (drives the wing pulse)

    var started = false
    var gameOver = false
    var deadT = 0f             // seconds since death (fall + card delay)

    var score = 0
    val stacks = mutableListOf<Stack>()
    private val rand = Random(7)

    val groundH = 96f
    val groundY: Float get() = screenH - groundH
    val birdR = 26f

    private val speed: Float get() = minOf(SPEED_MAX, SPEED_START + score * 3.5f)
    private fun gapFor(s: Int): Float = maxOf(GAP_MIN, GAP_START - s * 1.2f)

    fun reset(w: Float, h: Float) {
        screenW = w
        screenH = h
        birdX = w * 0.30f
        birdY = h * 0.42f
        vy = 0f
        rotation = 0f
        flapT = 99f
        started = false
        gameOver = false
        deadT = 0f
        score = 0
        stacks.clear()
        // First pillar starts on-screen (not off the right edge) so the
        // opening isn't a long empty flight — about 0.35·w from the bird.
        var x = w * 0.65f
        while (x < w + SPACING * 3) {
            stacks.add(newStack(x))
            x += SPACING
        }
    }

    private fun newStack(x: Float): Stack {
        val lo = 170f
        val hi = (groundY - 170f).coerceAtLeast(lo + 1f)
        return Stack(x, lo + rand.nextFloat() * (hi - lo), gapFor(score))
    }

    /** Tap! */
    fun flap() {
        if (gameOver) return
        started = true
        vy = FLAP_VY
        flapT = 0f
    }

    fun update(dt: Float) {
        if (dt <= 0f) return
        flapT += dt
        if (!started) return
        if (gameOver) {
            // tumble to the pier
            deadT += dt
            vy = minOf(vy + GRAVITY * dt, MAX_FALL * 1.3f)
            birdY += vy * dt
            rotation = minOf(90f, rotation + dt * 260f)
            if (birdY > groundY - birdR) {
                birdY = groundY - birdR
                vy = 0f
            }
            return
        }
        vy = minOf(vy + GRAVITY * dt, MAX_FALL)
        birdY += vy * dt
        // tilt: nose up on flap, nose down in a fall
        val targetRot = (vy / MAX_FALL).coerceIn(-1f, 1f) * 38f
        rotation += (targetRot - rotation) * minOf(1f, dt * 10f)
        // ceiling: bonk, don't die
        if (birdY < birdR) {
            birdY = birdR
            vy = 0f
        }

        val dx = speed * dt
        for (s in stacks) s.x -= dx
        stacks.removeAll { it.x < -PIPE_W - 40f }
        val last = stacks.maxOfOrNull { it.x } ?: 0f
        if (last < screenW + 40f) stacks.add(newStack(last + SPACING))

        for (s in stacks) {
            if (!s.scored && s.x + PIPE_W < birdX - birdR) {
                s.scored = true
                score++
            }
        }
        checkCollisions()
    }

    private fun checkCollisions() {
        // the pier deck
        if (birdY + birdR >= groundY) {
            gameOver = true
            return
        }
        // smokestack slabs (circle vs. vertical slabs — corners are forgiving)
        for (s in stacks) {
            if (birdX + birdR < s.x || birdX - birdR > s.x + PIPE_W) continue
            val top = s.gapY - s.gap / 2f
            val bot = s.gapY + s.gap / 2f
            if (birdY - birdR < top || birdY + birdR > bot) {
                gameOver = true
                return
            }
        }
    }

    companion object {
        const val GRAVITY = 2300f      // dp/s^2
        const val FLAP_VY = -780f      // dp/s
        const val MAX_FALL = 1150f     // dp/s
        const val PIPE_W = 86f         // dp
        const val GAP_START = 235f     // dp
        const val GAP_MIN = 185f       // dp
        const val SPEED_START = 270f   // dp/s
        const val SPEED_MAX = 440f     // dp/s
        const val SPACING = 370f       // dp between stacks
    }
}
