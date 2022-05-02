import React from 'react';
import { Route, Routes } from 'react-router-dom';

export const playListsPath = '/';
export const playListPath = 'playlist/:id';

const PlayLists = React.lazy(() => import('./pages/playlists'));
const PlayList = React.lazy(() => import('./pages/playlist'));
const NotFound = React.lazy(() => import('./pages/notfound'));
const Player = React.lazy(() => import('./components/player/player'));

export const AppRoutes: React.FC = () => {
    return (
        <Routes>
            <Route path={playListPath} element={<PlayList />} key="playlists" />
            <Route path={playListsPath} element={<PlayLists />} key="playlist" />
            <Route path="*" element={<NotFound />} key="notfound" />
        </Routes>
    );
};

export const PlayerRoutes: React.FC = () => {
    return (
        <Routes>
            <Route path={playListPath} element={<Player />} />
            <Route path={playListsPath} element={<Player />} />
        </Routes>
    );
};
