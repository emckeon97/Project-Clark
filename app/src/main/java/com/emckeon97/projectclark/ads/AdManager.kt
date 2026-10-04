package com.emckeon97.projectclark.ads

import android.app.Activity
import android.content.Context
import com.google.android.gms.ads.AdRequest
import com.google.android.gms.ads.AdSize
import com.google.android.gms.ads.AdView
import com.google.android.gms.ads.LoadAdError
import com.google.android.gms.ads.MobileAds
import com.google.android.gms.ads.interstitial.InterstitialAd
import com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback

/**
 * AdMob singleton for Project Clark.
 *
 * Ships with Google's TEST ad unit IDs ([useTestIDs] = true). When the real
 * AdMob units are provisioned, fill in the REAL_* constants and flip
 * [useTestIDs] to false — no other code changes needed.
 *
 * Kept deliberately light: banner on the menu only, interstitial on every
 * 5th game over (max one per 60s). Nothing during gameplay.
 */
object AdManager {

    const val useTestIDs = true

    // Google's official test ad units (safe to ship during development).
    private const val TEST_BANNER = "ca-app-pub-3940256099942544/6300978111"
    private const val TEST_INTERSTITIAL = "ca-app-pub-3940256099942544/1033173712"

    // TODO: replace with real AdMob ad units before release.
    const val REAL_BANNER = ""
    const val REAL_INTERSTITIAL = ""

    private val bannerID: String get() = if (useTestIDs) TEST_BANNER else REAL_BANNER
    private val interstitialID: String get() = if (useTestIDs) TEST_INTERSTITIAL else REAL_INTERSTITIAL

    private var interstitialAd: InterstitialAd? = null

    private var gameOverCount = 0
    private var lastInterstitialShownAt = 0L

    fun initialize(context: Context) {
        MobileAds.initialize(context) {}
    }

    /** Fresh banner view; host it in an AndroidView. */
    fun bannerView(context: Context): AdView =
        AdView(context).apply {
            setAdSize(AdSize.BANNER)
            adUnitId = bannerID
            loadAd(AdRequest.Builder().build())
        }

    fun loadInterstitial(context: Context) {
        if (interstitialAd != null) return
        InterstitialAd.load(
            context,
            interstitialID,
            AdRequest.Builder().build(),
            object : InterstitialAdLoadCallback() {
                override fun onAdLoaded(ad: InterstitialAd) {
                    interstitialAd = ad
                }

                override fun onAdFailedToLoad(error: LoadAdError) {
                    interstitialAd = null
                }
            }
        )
    }

    /** Returns true if an interstitial was shown. Preloads the next one. */
    fun showInterstitial(activity: Activity): Boolean {
        val ad = interstitialAd ?: return false
        ad.show(activity)
        interstitialAd = null
        lastInterstitialShownAt = System.currentTimeMillis()
        loadInterstitial(activity)
        return true
    }

    /**
     * Call on every game over. Shows an interstitial on every 5th game over,
     * at most once per 60 seconds — frequent enough to earn, rare enough
     * to keep players.
     */
    fun gameOverOccurred(activity: Activity?) {
        gameOverCount++
        if (activity == null) return
        val now = System.currentTimeMillis()
        if (gameOverCount % 5 == 0 && now - lastInterstitialShownAt > 60_000) {
            showInterstitial(activity)
        }
    }
}
