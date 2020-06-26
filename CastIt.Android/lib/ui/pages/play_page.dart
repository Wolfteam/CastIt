import 'package:flutter/material.dart';

class PlayPage extends StatelessWidget {
  var blueColor = Color(0xFF090e42);
  var pinkColor = Color(0xFFff6b80);
  var image =
      'https://i.scdn.co/image/db8382f6c33134111a26d4bf5a482a1caa5f151c';

  @override
  Widget build(BuildContext context) {
    var size = MediaQuery.of(context).size;
    var coverHeigth = size.height * 0.6;
    return Scaffold(
      body: Column(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Container(
            height: coverHeigth,
            child: Stack(
              children: <Widget>[
                Container(
                  decoration: BoxDecoration(
                    image: DecorationImage(
                      image: NetworkImage(image),
                      fit: BoxFit.cover,
                    ),
                  ),
                ),
                Container(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      colors: [blueColor.withOpacity(0.4), blueColor],
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                    ),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12.0),
                  child: Column(
                    children: <Widget>[
                      SizedBox(height: 20.0),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: <Widget>[
                          Container(
                            decoration: BoxDecoration(
                              color: Colors.white.withOpacity(0.1),
                              borderRadius: BorderRadius.circular(50.0),
                            ),
                            child: Icon(
                              Icons.arrow_drop_down,
                              color: Colors.white,
                            ),
                          ),
                          Column(
                            children: <Widget>[
                              Text(
                                'PLAYLIST',
                                style: TextStyle(
                                    color: Colors.white.withOpacity(0.6)),
                              ),
                              Text('Best Vibes of the Week',
                                  style: TextStyle(color: Colors.white)),
                            ],
                          ),
                          Icon(
                            Icons.playlist_add,
                            color: Colors.white,
                          )
                        ],
                      ),
                      Spacer(),
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: <Widget>[
                          Icon(
                            Icons.shuffle,
                            color: pinkColor,
                          ),
                          Column(
                            children: <Widget>[
                              Text("Titulo",
                                  style: TextStyle(
                                    color: Colors.white,
                                    fontWeight: FontWeight.bold,
                                    fontSize: 32.0,
                                  )),
                              SizedBox(
                                height: 6.0,
                              ),
                              Text(
                                "Artista",
                                style: TextStyle(
                                  color: Colors.white.withOpacity(0.6),
                                  fontSize: 18.0,
                                ),
                              ),
                              SizedBox(height: 16.0),
                            ],
                          ),
                          Icon(
                            Icons.repeat,
                            color: pinkColor,
                          ),
                        ],
                      ),
                    ],
                  ),
                )
              ],
            ),
          ),
          SizedBox(height: 20.0),
          Slider(
            onChanged: (double value) {},
            value: 0.2,
            activeColor: pinkColor,
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: <Widget>[
                Text(
                  '2:10',
                  style: TextStyle(color: Colors.green.withOpacity(0.7)),
                ),
                Text('-03:56',
                    style: TextStyle(color: Colors.green.withOpacity(0.7)))
              ],
            ),
          ),
          SizedBox(height: 10),
          Container(
            margin: EdgeInsets.only(bottom: 50),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: <Widget>[
                IconButton(
                  iconSize: 42,
                  onPressed: () {},
                  icon: Icon(
                    Icons.fast_rewind,
                    color: Colors.black,
                  ),
                ),
                IconButton(
                  iconSize: 42,
                  onPressed: () {},
                  icon: Icon(
                    Icons.skip_previous,
                    color: Colors.black,
                  ),
                ),
                SizedBox(width: 10.0),
                Container(
                  decoration: BoxDecoration(
                    color: pinkColor,
                    borderRadius: BorderRadius.circular(50.0),
                  ),
                  child: IconButton(
                    iconSize: 60,
                    onPressed: () {},
                    icon: Icon(
                      Icons.play_arrow,
                      color: Colors.white,
                    ),
                  ),
                ),
                SizedBox(width: 10.0),
                IconButton(
                  iconSize: 42,
                  onPressed: () {},
                  icon: Icon(
                    Icons.skip_next,
                    color: Colors.black,
                  ),
                ),
                IconButton(
                  iconSize: 42,
                  onPressed: () {},
                  icon: Icon(
                    Icons.fast_forward,
                    color: Colors.black,
                  ),
                ),
              ],
            ),
          ),

          // Row(
          //   mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          //   children: <Widget>[
          //     Icon(
          //       Icons.bookmark_border,
          //       color: pinkColor,
          //     ),
          //     Icon(
          //       Icons.shuffle,
          //       color: pinkColor,
          //     ),
          //     Icon(
          //       Icons.repeat,
          //       color: pinkColor,
          //     ),
          //   ],
          // ),
        ],
      ),
    );
  }
}
