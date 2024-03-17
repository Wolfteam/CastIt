import { Dialog, DialogTitle, DialogContent, DialogActions, Button, ListItem, List, Typography, useTheme } from '@mui/material';
import { useEffect, useState } from 'react';
import translations from '../../services/translations';
import { useCastItHub } from '../../context/castit_hub.context';
import { IGetAllPlayListResponseDto } from '../../models';
import { DragDropContext, Draggable, Droppable, DropResult } from '@hello-pangea/dnd';
import { DragIndicator } from '@mui/icons-material';

interface Props {
    isOpen: boolean;
    playLists: IGetAllPlayListResponseDto[];
    onClose(): void;
}

interface IPlayListPosition {
    id: number;
    name: string;
    position: number;
}

interface State {
    positions: IPlayListPosition[];
}

function ReOrderPlayListDialog(props: Props) {
    const [state, setstate] = useState<State>({
        positions: [],
    });
    const castItHub = useCastItHub();

    useEffect(() => {
        const positions: IPlayListPosition[] = props.playLists.map((pl) => {
            return {
                id: pl.id,
                name: pl.name,
                position: pl.position,
            };
        });
        setstate({ positions: positions });
    }, [props]);

    const handleDragEnd = async (result: DropResult): Promise<void> => {
        if (!result.destination) {
            return;
        }

        if (result.destination!.droppableId === result.source.droppableId && result.destination.index === result.source.index) {
            return;
        }

        const playLists = { ...props.playLists };
        const source = playLists[result.source.index];
        if (!source) {
            return;
        }

        await castItHub.connection.updatePlayListPosition(source.id, result.destination!.index);
    };

    if (!props.isOpen) {
        return null;
    }

    const items = state.positions.map((item, index) => {
        return <Item key={item.id} index={index} playlist={item} />;
    });

    return (
        <Dialog open={props.isOpen} onClose={() => props.onClose()} maxWidth="sm" fullWidth>
            <DialogTitle>{translations.sort}</DialogTitle>
            <DialogContent>
                <DragDropContext onDragEnd={handleDragEnd}>
                    <Droppable droppableId="playlists-droppable" direction="vertical">
                        {(provided) => (
                            <List {...provided.droppableProps} ref={provided.innerRef}>
                                {items}
                                {provided.placeholder}
                            </List>
                        )}
                    </Droppable>
                </DragDropContext>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => props.onClose()} variant="contained" color="primary">
                    {translations.ok}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

interface ItemProps {
    index: number;
    playlist: IPlayListPosition;
}

function Item(props: ItemProps) {
    const theme = useTheme();
    return (
        <Draggable index={props.index} draggableId={`${props.playlist.id}_${props.playlist.name}`}>
            {(provided, snapshot) => (
                <div ref={provided.innerRef} {...provided.dragHandleProps} {...provided.draggableProps}>
                    <ListItem
                        style={{
                            backgroundColor: snapshot.isDragging ? theme.palette.primary.dark : '',
                        }}
                    >
                        <DragIndicator />
                        <Typography>{props.playlist.name}</Typography>
                    </ListItem>
                </div>
            )}
        </Draggable>
    );
}

export default ReOrderPlayListDialog;
