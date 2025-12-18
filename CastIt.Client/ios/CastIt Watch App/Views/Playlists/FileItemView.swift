//
//  FileItemView.swift
//  castIt
//
//  Created by Efrain Bastidas on 16/12/25.
//

import SwiftUI

struct FileItemView: View {
    private let container: DependencyContainer
    @State private var viewModel: FileItemViewModel
    @Environment(\.dismiss) private var dismiss

    init(container: DependencyContainer, item: FileItemResponseDto) {
        self.container = container
        _viewModel = State(initialValue: container.makeFileItemViewModel(file: item))
    }

    var body: some View {
        VStack(alignment: .leading) {
            Text(viewModel.file.name)
                .font(.caption2)
                .fontWeight(.bold)
                .lineLimit(2)
                .foregroundStyle(viewModel.file.isBeingPlayed ? .red : .primary)
            Text(viewModel.file.subTitle)
                .font(.footnote)
                .fontWeight(.ultraLight)
                .lineLimit(1)
            HStack (alignment: .center) {
                Text(viewModel.file.playedTime)
                    .font(.footnote)
                    .lineLimit(1)
                Spacer()
                Text(viewModel.file.duration)
                    .font(.footnote)
                    .lineLimit(1)
            }
            ProgressView(value: viewModel.file.playedPercentage, total: 100)
                .frame(height: 2)
                .clipShape(Capsule())
                .progressViewStyle(LinearProgressViewStyle())
        }
        .onTapGesture {
            // Start playback
            viewModel.play()
            // Close this view (pop PlaylistDetailView's stack) and switch back to Player tab
            dismiss()
            container.router.selectedTab = .player
        }
    }
}
