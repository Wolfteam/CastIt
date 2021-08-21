import { IconButton, Tooltip } from '@material-ui/core';
import { Loop, Shuffle } from '@material-ui/icons';
import { useContext } from 'react';
import { CastItHubContext } from '../../context/castit_hub.context';
import translations from '../../services/translations';

interface Props {
    id: number;
    loop?: boolean;
    shuffle?: boolean;
    renderLoop?: boolean;
}

function PlayListLoopShuffleButton(props: Props) {
    const [castItHub] = useContext(CastItHubContext);

    const handleOptionChanged = async (loop?: boolean, shuffle?: boolean): Promise<void> => {
        await castItHub.connection.setPlayListOptions(props.id, loop!, shuffle!);
    };

    if (props.renderLoop) {
        const color = props.loop ? 'primary' : undefined;
        return (
            <Tooltip title={translations.loop}>
                <IconButton onClick={() => handleOptionChanged(!props.loop, props.shuffle)}>
                    <Loop color={color} />
                </IconButton>
            </Tooltip>
        );
    }

    const color = props.shuffle ? 'primary' : undefined;
    return (
        <Tooltip title={translations.shuffle}>
            <IconButton onClick={() => handleOptionChanged(props.loop, !props.shuffle)}>
                <Shuffle color={color} />
            </IconButton>
        </Tooltip>
    );
}

export default PlayListLoopShuffleButton;
