import SwiftUI

struct PlayerImage: View {
    private let imageSize: Double = 80;
    @State private var viewModel: PlayerViewModel
    
    init(viewModel: PlayerViewModel) {
        _viewModel = State(initialValue: viewModel)
    }
    
    var body: some View {
        if let urlStr = viewModel.playedFile?.thumbnailUrl, let url = URL(string: urlStr) {
            AsyncImage(url: url) { phase in
                switch phase {
                case .empty:
                    ProgressView()
                case .success(let image):
                    image
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                case .failure:
                    Image(systemName: "photo")
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                @unknown default:
                    Image(systemName: "photo")
                        .resizable()
                        .aspectRatio(contentMode: .fill)
                }
            }
            .frame(width: imageSize, height: imageSize)
            .clipShape(RoundedRectangle(cornerRadius: 8))
        } else {
            Image(systemName: "photo")
                .resizable()
                .aspectRatio(contentMode: .fill)
                .frame(width: imageSize, height: imageSize)
        }
    }
}
