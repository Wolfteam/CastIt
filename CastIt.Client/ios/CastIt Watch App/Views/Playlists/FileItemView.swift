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
            Text(viewModel.filename)
                .font(.caption2)
                .fontWeight(.bold)
                .lineLimit(2)
                .foregroundStyle(viewModel.isBeingPlayed ? .red : .primary)
            Text(viewModel.subTitle)
                .font(.footnote)
                .fontWeight(.ultraLight)
                .lineLimit(1)
            HStack (alignment: .center) {
                Text(viewModel.playedTime)
                    .font(.footnote)
                    .lineLimit(1)
                Spacer()
                Text(viewModel.duration)
                    .font(.footnote)
                    .lineLimit(1)
            }
            ProgressView(value: viewModel.playedPercentage, total: 100)
                .frame(height: 2)
                .clipShape(Capsule())
                .progressViewStyle(LinearProgressViewStyle())
        }
        .contentShape(Rectangle())
        .onTapGesture {
            // Start playback
            viewModel.play()
            // Close this view (pop PlaylistDetailView's stack) and switch back to Player tab
            dismiss()
            container.router.selectedTab = .player
        }
        .swipeActions(edge: .trailing, allowsFullSwipe: false) {
            Button {
                viewModel.showDeleteConfirmation = true
            } label: {
                Label("Delete", systemImage: "trash")
            }
            .tint(.red)
            
            if viewModel.isBeingPlayed {
                Button {
                    viewModel.toggleLoop()
                } label: {
                    Label(viewModel.loop ? "Disable Loop" : "Enable Loop", systemImage: viewModel.loop ? "repeat.1.circle.fill" : "repeat.1.circle")
                }
                .tint(viewModel.loop ? .green : .gray)
            }
        }
        .swipeActions(edge: .leading, allowsFullSwipe: false) {
            Button {
                viewModel.play(force: true)
                dismiss()
                container.router.selectedTab = .player
            } label: {
                Label("Play from beginning", systemImage: "play.circle")
            }
            .tint(.orange)
            
            Button {
                viewModel.play(force: false)
                dismiss()
                container.router.selectedTab = .player
            } label: {
                Label("Play file", systemImage: "play")
            }
            .tint(.blue)
        }
        .confirmationDialog("Delete File?", isPresented: $viewModel.showDeleteConfirmation) {
            Button("Delete", role: .destructive) {
                viewModel.delete()
            }
            Button("Cancel", role: .cancel) {}
        } message: {
            Text("Are you sure you want to delete '\(viewModel.filename)'?")
        }
    }
}
