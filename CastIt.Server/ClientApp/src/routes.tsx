import React from 'react';
import { Route, Switch } from 'react-router-dom';

export const playListsPath = '/';
export const playListPath = '/playlist/:id';

const PlayLists = React.lazy(() => import('./pages/playlists'));
const PlayList = React.lazy(() => import('./pages/playlist'));
const NotFound = React.lazy(() => import('./pages/notfound'));
const Player = React.lazy(() => import('./components/player/player'));

export const AppRoutes: React.FC = () => {
    return (
        <Switch>
            <Route exact path={playListPath} component={PlayList} />
            <Route exact path={playListsPath} component={PlayLists} />
            <Route path="*" component={NotFound} />
        </Switch>
    );
};

export const PlayerRoutes: React.FC = () => {
    return (
        <Switch>
            <Route exact path={playListPath} component={Player} />
            <Route exact path={playListsPath} component={Player} />
        </Switch>
    );
};
