import SwiftUI

/// Bottom banner ad (50pt tall, matches the Android banner slot).
struct AdBannerView: View {
    var body: some View {
        AdBannerRepresentable()
            .frame(height: 50)
    }
}

/// Hosts the AdMob banner view controller built by AdManager.
private struct AdBannerRepresentable: UIViewControllerRepresentable {
    func makeUIViewController(context: Context) -> UIViewController {
        AdManager.shared.makeBanner()
    }

    func updateUIViewController(_ uiViewController: UIViewController, context: Context) {}
}
