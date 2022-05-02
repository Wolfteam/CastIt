import { useCallback, useEffect, useState } from 'react';
import { IGetAllPlayListResponseDto } from '../models';
import {
    onPlayListAdded,
    onPlayListsLoaded,
    onPlayListDeleted,
    onPlayListChanged,
    onPlayListsChanged,
    onFileLoading,
    onFileLoaded,
    onFileEndReached,
} from '../services/castithub.service';
import PlayListCardItem from '../components/playlist/playlist_card_item';
import PageContent from './page_content';
import { Grid } from '@material-ui/core';
import ReOrderPlayListDialog from '../components/dialogs/reorder_playlist_dialog';
import { useCastItHub } from '../context/castit_hub.context';

interface State {
    isBusy: boolean;
    playLists: IGetAllPlayListResponseDto[];
}

const initialState: State = {
    isBusy: true,
    playLists: [],
};

function PlayLists() {
    const [state, setState] = useState(initialState);
    const [showReorderDialog, setShowReorderDialog] = useState(false);

    const castItHub = useCastItHub();

    useEffect(() => {
        const onPlayListAddedSubscription = onPlayListAdded.subscribe((playList) => {
            const copy = [...state.playLists];
            copy.push(playList);
            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListsChangedSubscription = onPlayListsChanged.subscribe((playLists) => {
            setState((s) => ({ ...s, playLists: playLists }));
        });

        const onPlayListChangedSubscription = onPlayListChanged.subscribe((playList) => {
            const copy = [...state.playLists];
            const index = copy.findIndex((pl) => pl.id === playList.id);
            if (index < 0) {
                return;
            }

            copy.splice(index, 1);
            copy.splice(index, 0, playList);

            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListDeletedSubscription = onPlayListDeleted.subscribe((id) => {
            const copy = [...state.playLists];
            const index = copy.findIndex((pl) => pl.id === id);
            if (index < 0) {
                return;
            }
            copy.splice(index, 1);
            setState((s) => ({ ...s, playLists: copy }));
        });

        return () => {
            onPlayListAddedSubscription.unsubscribe();
            onPlayListsChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();
            onPlayListDeletedSubscription.unsubscribe();
        };
    }, [state.playLists]);

    useEffect(() => {
        castItHub.connection.getAllPlayLists().then((playLists) => {
            setState({ isBusy: false, playLists: playLists });
        });
    }, [castItHub.connection]);

    useEffect(() => {
        const onFileLoadingSubscription = onFileLoading.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: true }));
        });

        const onFileLoadedSubscription = onFileLoaded.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        return () => {
            onFileLoadingSubscription.unsubscribe();
            onFileLoadedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, []);

    const handleReOrderClick = useCallback(() => {
        setShowReorderDialog(true);
    }, []);

    const items = state.playLists.map((pl, index) => (
        <Grid key={pl.id} item xs={12} sm={4} md={4} lg={3} xl={2}>
            <PlayListCardItem index={index} playList={pl} onReOrderClick={handleReOrderClick} />
        </Grid>
    ));

    const addNew = (
        <Grid key="AddNewItem" item xs={12} sm={4} md={4} lg={3} xl={2} style={{ alignSelf: 'center' }}>
            <PlayListCardItem
                playList={{
                    id: 0,
                    position: -1,
                    name: '',
                    imageUrl: '',
                    numberOfFiles: -1,
                    totalDuration: '',
                    loop: false,
                    shuffle: false,
                    playedTime: '',
                }}
                toAddNewItem
                index={items.length + 1}
            />
        </Grid>
    );

    items.push(addNew);

    return (
        <PageContent useContainer>
            <Grid container spacing={3} style={{ marginTop: 20, marginBottom: 20 }}>
                {items}
                <ReOrderPlayListDialog isOpen={showReorderDialog} onClose={() => setShowReorderDialog(false)} playLists={state.playLists} />
            </Grid>
        </PageContent>
    );
}

export default PlayLists;
