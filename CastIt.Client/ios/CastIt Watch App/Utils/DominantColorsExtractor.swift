import SwiftUI
import CoreGraphics
import ImageIO

enum DominantColorsExtractor {
    static func dominantColors(from data: Data, maxDimension: Int = 10) -> [Color] {
        guard let src = CGImageSourceCreateWithData(data as CFData, nil),
              let cgImage = CGImageSourceCreateImageAtIndex(src, 0, nil) else { return [] }

        let width = min(maxDimension, cgImage.width)
        let height = min(maxDimension, cgImage.height)
        let bitsPerComponent = 8
        let bytesPerPixel = 4
        let bytesPerRow = width * bytesPerPixel
        let colorSpace = CGColorSpaceCreateDeviceRGB()
        let bitmapInfo = CGBitmapInfo(rawValue: CGImageAlphaInfo.premultipliedLast.rawValue)

        guard let context = CGContext(
            data: nil,
            width: width,
            height: height,
            bitsPerComponent: bitsPerComponent,
            bytesPerRow: bytesPerRow,
            space: colorSpace,
            bitmapInfo: bitmapInfo.rawValue
        ) else { return [] }

        // Draw scaled image into context
        context.interpolationQuality = .low
        context.draw(cgImage, in: CGRect(x: 0, y: 0, width: width, height: height))

        guard let dataPtr = context.data else { return [] }
        let pixelBuffer = dataPtr.bindMemory(to: UInt8.self, capacity: width * height * bytesPerPixel)

        var histogram: [UInt32: Int] = [:]
        func quantize(_ r: UInt8, _ g: UInt8, _ b: UInt8) -> UInt32 {
            // Reduce color space to 5 bits per channel to stabilize dominance
            let rq = UInt32(r) >> 3
            let gq = UInt32(g) >> 3
            let bq = UInt32(b) >> 3
            return (rq << 10) | (gq << 5) | bq
        }

        for y in 0..<height {
            for x in 0..<width {
                let idx = (y * width + x) * bytesPerPixel
                let r = pixelBuffer[idx]
                let g = pixelBuffer[idx + 1]
                let b = pixelBuffer[idx + 2]
                let a = pixelBuffer[idx + 3]
                // Skip transparent pixels
                if a < 10 { continue }
                let key = quantize(r, g, b)
                histogram[key, default: 0] += 1
            }
        }

        // Sort by frequency and pick top 2
        let sorted = histogram.sorted { $0.value > $1.value }
        let top = Array(sorted.prefix(2))

        func color(from key: UInt32) -> Color {
            let rq = (key >> 10) & 0x1F
            let gq = (key >> 5) & 0x1F
            let bq = key & 0x1F
            // Expand back to 8-bit space
            let r = Double((rq << 3) | (rq >> 2)) / 255.0
            let g = Double((gq << 3) | (gq >> 2)) / 255.0
            let b = Double((bq << 3) | (bq >> 2)) / 255.0
            return Color(red: r, green: g, blue: b)
        }

        let colors = top.map { color(from: $0.key) }
        if colors.isEmpty { return [] }
        if colors.count == 1 { return [colors[0], colors[0].opacity(0.7)] }
        return colors
    }
}
