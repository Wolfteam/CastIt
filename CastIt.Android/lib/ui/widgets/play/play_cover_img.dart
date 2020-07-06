import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

import '../../../common/extensions/string_extensions.dart';

import '../../../generated/i18n.dart';

class PlayCoverImg extends StatelessWidget {
  final int fileId;
  final String thumbUrl;
  final int playListId;
  final String playListName;
  final String fileName;
  final bool showLoading;

  const PlayCoverImg({
    Key key,
    this.fileId,
    this.playListId,
    this.playListName,
    this.fileName,
    this.thumbUrl,
    this.showLoading = false,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final i18n = I18n.of(context);
    final size = MediaQuery.of(context).size;
    final coverHeigth = size.height * 0.6;
    return Container(
      height: coverHeigth,
      child: Stack(
        children: <Widget>[
          if (!thumbUrl.isNullEmptyOrWhitespace && !showLoading)
            CachedNetworkImage(
              imageUrl: thumbUrl,
              imageBuilder: (ctx, imageProvider) => Container(
                decoration: BoxDecoration(
                  image: DecorationImage(
                    image: imageProvider,
                    fit: BoxFit.fill,
                  ),
                ),
              ),
              placeholder: (ctx, url) => const CircularProgressIndicator(),
              errorWidget: (ctx, url, error) => Container(),
            ),
          if (showLoading) const Center(child: CircularProgressIndicator()),
          Container(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [Colors.black.withOpacity(0.1), Colors.black.withOpacity(0.5)],
                begin: Alignment.center,
                end: Alignment.topCenter,
              ),
            ),
          ),
          Container(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [Colors.black.withOpacity(0.1), Colors.black.withOpacity(0.8)],
                begin: Alignment.center,
                end: Alignment.bottomCenter,
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 10.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                const SizedBox(height: 20.0),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: <Widget>[
                    Container(
                      decoration: BoxDecoration(
                        color: Colors.white.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(50.0),
                      ),
                      child: IconButton(
                        icon: Icon(
                          Icons.playlist_play,
                          color: Colors.white,
                        ),
                        onPressed: () {},
                      ),
                    ),
                    Column(
                      children: <Widget>[
                        Text(
                          i18n.playlist,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(
                            color: Colors.white.withOpacity(0.6),
                          ),
                        ),
                        Text(
                          playListName.isNullEmptyOrWhitespace ? '' : playListName,
                          overflow: TextOverflow.ellipsis,
                          style: TextStyle(color: Colors.white),
                        ),
                      ],
                    ),
                    IconButton(
                      icon: Icon(
                        Icons.settings,
                        color: Colors.white,
                      ),
                      onPressed: _goToPlayList,
                    )
                  ],
                ),
                const Spacer(),
                Container(
                  margin: const EdgeInsets.symmetric(vertical: 16),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: <Widget>[
                      IconButton(
                        icon: Icon(
                          Icons.shuffle,
                          color: Colors.white,
                        ),
                        onPressed: () {},
                      ),
                      Flexible(
                        child: Text(
                          fileName.isNullEmptyOrWhitespace ? '' : fileName,
                          overflow: TextOverflow.ellipsis,
                          textAlign: TextAlign.center,
                          style: TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 28.0,
                          ),
                        ),
                      ),
                      IconButton(
                        icon: Icon(
                          Icons.repeat,
                          color: Colors.white,
                        ),
                        onPressed: () {},
                      ),
                    ],
                  ),
                ),
              ],
            ),
          )
        ],
      ),
    );
  }

  void _goToPlayList() {
    if (playListId == null) return;
  }
}
