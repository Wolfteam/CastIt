import { Dialog, DialogTitle, DialogContent, DialogActions, Button, ListItem, List, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import translations from '../../services/translations';
import { useCastItHub } from '../../context/castit_hub.context';
import { IGetAllPlayListResponseDto } from '../../models';
import { DragDropContext, Draggable, Droppable, DropResult } from 'react-beautiful-dnd';
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

    const items = state.positions.map((item, index) => (
        <Draggable index={index} draggableId={`${item.id}_${item.name}`}>
            {(provided) => (
                <ListItem ref={provided.innerRef} {...provided.dragHandleProps} {...provided.draggableProps}>
                    <DragIndicator />
                    <Typography>{item.name}</Typography>
                </ListItem>
            )}
        </Draggable>
    ));

    return (
        <Dialog open={props.isOpen} onClose={() => props.onClose()} maxWidth="sm" fullWidth>
            <DialogTitle>{translations.sort}</DialogTitle>
            <DialogContent>
                <DragDropContext onDragEnd={handleDragEnd}>
                    <Droppable droppableId="playlists-droppable" direction="vertical">
                        {(provided) => (
                            <List {...provided.droppableProps}>
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

export default ReOrderPlayListDialog;
