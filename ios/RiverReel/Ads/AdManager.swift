//
//  AdManager.swift
//  River Reel
//
//  Google Mobile Ads (AdMob) integration — GMA iOS SDK v13 Swift API naming.
//  - Banner: bottom of the menu.
//  - Interstitial: every 5th game over, max once per 60 seconds.
//
//  DEVELOPMENT: uses Google's official test ad unit IDs (useTestIDs = true).
//  PRODUCTION: set useTestIDs = false and paste the real unit IDs into
//  REAL_BANNER_ID / REAL_INTERSTITIAL_ID.
//
//  The AdMob APP ID (ca-app-pub-3940256099942544~1458002511 for tests)
//  belongs in Info.plist under the GADApplicationIdentifier key — not here.
//
//  Mac Catalyst: Google Mobile Ads has no Catalyst slice, so the SDK is only
//  linked to the iOS target and every SDK reference below is behind
//  #if !targetEnvironment(macCatalyst) — on Mac the class compiles to no-ops.
//

import Combine
import UIKit

#if !targetEnvironment(macCatalyst)
import GoogleMobileAds
#endif

final class AdManager: NSObject, ObservableObject {

    static let shared = AdManager()

    /// Flip to false and fill in REAL_* below when production units are created.
    static let useTestIDs = true

    static let REAL_BANNER_ID = ""
    static let REAL_INTERSTITIAL_ID = ""

    // Google's official sample IDs — safe for development.
    private static let TEST_BANNER_ID = "ca-app-pub-3940256099942544/2934735716"
    private static let TEST_INTERSTITIAL_ID = "ca-app-pub-3940256099942544/4411468910"

    private static var bannerID: String {
        useTestIDs ? TEST_BANNER_ID : REAL_BANNER_ID
    }
    private static var interstitialID: String {
        useTestIDs ? TEST_INTERSTITIAL_ID : REAL_INTERSTITIAL_ID
    }

    @Published var isInterstitialReady = false

    private var gameOverCount = 0
    private var lastInterstitialShownAt: Date?

    #if !targetEnvironment(macCatalyst)
    private var interstitialAd: InterstitialAd?
    #endif

    override init() {
        super.init()
        #if !targetEnvironment(macCatalyst)
        MobileAds.shared.start()
        loadInterstitial()
        #endif
    }

    // MARK: - Banner

    /// Returns a view controller hosting a 320x50 banner that loads on appear.
    /// On Catalyst this is a plain empty view controller (no ad SDK on Mac).
    func makeBanner() -> UIViewController {
        #if !targetEnvironment(macCatalyst)
        let vc = AdBannerViewController()
        vc.adUnitID = Self.bannerID
        return vc
        #else
        return UIViewController()
        #endif
    }

    // MARK: - Interstitial

    func loadInterstitial() {
        #if !targetEnvironment(macCatalyst)
        let id = Self.interstitialID
        guard !id.isEmpty else { return }
        InterstitialAd.load(with: id, request: Request()) { [weak self] ad, error in
            DispatchQueue.main.async {
                guard let self else { return }
                if let ad {
                    ad.fullScreenContentDelegate = self
                    self.interstitialAd = ad
                    self.isInterstitialReady = true
                } else {
                    self.isInterstitialReady = false
                }
            }
        }
        #endif
    }

    /// Presents the interstitial if one is loaded and the 60-second cooldown
    /// has elapsed since the last presentation. Returns true if presented.
    /// Always false on Catalyst.
    @discardableResult
    func showInterstitial(from rootViewController: UIViewController) -> Bool {
        #if !targetEnvironment(macCatalyst)
        if let last = lastInterstitialShownAt,
           Date().timeIntervalSince(last) < 60 {
            return false
        }
        guard let ad = interstitialAd else { return false }
        lastInterstitialShownAt = Date()
        interstitialAd = nil
        isInterstitialReady = false
        ad.present(from: rootViewController)
        return true
        #else
        return false
        #endif
    }

    /// Call on every game over. Shows an interstitial on every 5th game over
    /// (subject to the 60-second cooldown) and keeps the next one preloading
    /// otherwise. Finds the presenting view controller internally.
    /// No-op on Catalyst.
    func gameOverOccurred() {
        #if !targetEnvironment(macCatalyst)
        gameOverCount += 1
        guard gameOverCount % 5 == 0 else {
            if interstitialAd == nil { loadInterstitial() }
            return
        }
        if let rootVC = Self.topViewController(), showInterstitial(from: rootVC) {
            // The next ad preloads in adDidDismissFullScreenContent.
        } else {
            loadInterstitial()
        }
        #endif
    }

    // MARK: - Helpers

    private static func topViewController() -> UIViewController? {
        let scenes = UIApplication.shared.connectedScenes.compactMap { $0 as? UIWindowScene }
        for scene in scenes {
            var vc = scene.windows.first(where: { $0.isKeyWindow })?.rootViewController
            while let presented = vc?.presentedViewController { vc = presented }
            if vc != nil { return vc }
        }
        return nil
    }
}

#if !targetEnvironment(macCatalyst)
// MARK: - FullScreenContentDelegate

extension AdManager: FullScreenContentDelegate {
    func adDidDismissFullScreenContent(_ ad: FullScreenPresentingAd) {
        loadInterstitial()
    }

    func ad(_ ad: FullScreenPresentingAd,
            didFailToPresentFullScreenContentWithError error: Error) {
        loadInterstitial()
    }
}

// MARK: - Banner view controller

/// Hosts a GADBannerView pinned to the bottom of its view.
final class AdBannerViewController: UIViewController {
    var adUnitID: String = ""

    override func viewDidLoad() {
        super.viewDidLoad()
        let banner = BannerView(adSize: AdSizeBanner)
        banner.adUnitID = adUnitID
        banner.rootViewController = self
        banner.translatesAutoresizingMaskIntoConstraints = false
        view.addSubview(banner)
        NSLayoutConstraint.activate([
            banner.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            banner.bottomAnchor.constraint(equalTo: view.bottomAnchor),
            banner.widthAnchor.constraint(equalToConstant: 320),
            banner.heightAnchor.constraint(equalToConstant: 50),
        ])
        banner.load(Request())
    }
}
#endif
