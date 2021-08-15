import { Container } from '@material-ui/core';
import { Fragment, useEffect, useState } from 'react';
import { IGetAllPlayListResponseDto } from '../models';
import {
    initializeHubConnection,
    onPlayListAdded,
    onPlayListsLoaded,
    onPlayListDeleted,
    onPlayListChanged,
    onPlayListsChanged,
    onFileLoading,
    onFileLoaded,
    onFileEndReached,
    onPlayerStatusChanged,
    updatePlayListPosition,
} from '../services/castithub.service';
import PlayListCardItem from '../components/playlist/playlist_card_item';
import PageContent from './page_content';
import { Grid } from '@material-ui/core';
import { DragDropContext, Draggable, Droppable, DropResult } from 'react-beautiful-dnd';

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

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status.playList) {
                return;
            }

            const playList = state.playLists.find((pl) => pl.id === status.playList!.id);
            if (!playList) {
                return;
            }

            const index = state.playLists.indexOf(playList);
            const copy = [...state.playLists];
            copy.splice(index, 1);
            copy.splice(index, 0, status.playList!);
            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListAddedSubscription = onPlayListAdded.subscribe((playList) => {
            const copy = [...state.playLists];
            copy.splice(playList.position, 0, playList);
            setState((s) => ({ ...s, playLists: copy }));
        });

        const onPlayListsChangedSubscription = onPlayListsChanged.subscribe(playLists => {
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
            onPlayerStatusChangedSubscription.unsubscribe();
            onPlayListsChangedSubscription.unsubscribe();
            onPlayListChangedSubscription.unsubscribe();
            onPlayListDeletedSubscription.unsubscribe();
        };
    }, [state.playLists]);

    useEffect(() => {
        const onPlayListsLoadedSubscription = onPlayListsLoaded.subscribe((playLists) => {
            setState({ isBusy: false, playLists: playLists });
        });

        const onFileLoadingSubscription = onFileLoading.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: true }));
        });

        const onFileLoadedSubscription = onFileLoaded.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((_) => {
            setState((s) => ({ ...s, isBusy: false }));
        });

        initializeHubConnection();

        return () => {
            onPlayListsLoadedSubscription.unsubscribe();
            onFileLoadingSubscription.unsubscribe();
            onFileLoadedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, []);

    const items = state.playLists.map((pl, index) => (
        <Grid key={pl.id} item xs={6} sm={6} md={4} lg={3} xl={2}>
            <PlayListCardItem {...pl} index={index} />
        </Grid>
    ));

    const addNew = (
        <Grid key="AddNewItem" item xs={6} sm={6} md={4} lg={3} xl={2} style={{ alignSelf: 'center' }}>
            <PlayListCardItem
                id={0}
                position={-1}
                name=""
                imageUrl=""
                numberOfFiles={-1}
                totalDuration=""
                toAddNewItem
                index={items.length + 1}
            />
        </Grid>
    );

    const handleDragEnd = async (result: DropResult): Promise<void> => {
        if (!result.destination) {
            return;
        }

        if (result.destination!.droppableId === result.source.droppableId && result.destination.index === result.source.index) {
            return;
        }

        const playLists = { ...state.playLists };
        const source = playLists[result.source.index];
        if (!source) {
            return;
        }

        await updatePlayListPosition(source.id, result.destination!.index);
    };

    items.push(addNew);

    return (
        <PageContent useContainer>
            <DragDropContext onDragEnd={handleDragEnd}>
                <Droppable droppableId="playlists-droppable" direction="horizontal">
                    {(provided) => (
                        <Grid
                            container
                            spacing={3}
                            style={{ marginTop: 20, marginBottom: 20 }}
                            {...provided.droppableProps}
                            innerRef={provided.innerRef}
                        >
                            {items}
                            {provided.placeholder}
                        </Grid>
                    )}
                </Droppable>
            </DragDropContext>
        </PageContent>
    );
}

export default PlayLists;
