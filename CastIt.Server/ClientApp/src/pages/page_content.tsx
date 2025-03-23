import { Container } from '@mui/material';
import { styled } from '@mui/material/styles';
import { JSX } from 'react';
const PREFIX = 'PageContent';

const classes = {
    root: `${PREFIX}-root`,
};

const Root = styled('div')((_) => ({
    [`&.${classes.root}`]: {
        flex: 'auto',
        overflowY: 'auto',
    },
}));

interface Props {
    useContainer?: boolean;
    children: JSX.Element;
}

function PageContent(props: Props) {
    if (props.useContainer) {
        return (
            <Root className={classes.root}>
                <Container id="page-content" sx={{ maxWidth: '2000px !important' }}>
                    {props.children}
                </Container>
            </Root>
        );
    }
    return (
        <Root id="page-content" className={classes.root}>
            {props.children}
        </Root>
    );
}

export default PageContent;
