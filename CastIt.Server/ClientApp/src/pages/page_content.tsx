import { Container, createStyles, makeStyles } from '@material-ui/core';

const useStyles = makeStyles((theme) =>
    createStyles({
        root: {
            flex: 'auto',
            overflowY: 'auto',
        },
    })
);

interface Props {
    useContainer?: boolean;
    children: JSX.Element;
}

function PageContent(props: Props) {
    const classes = useStyles();
    if (props.useContainer) {
        return (
            <Container id="page-content" maxWidth="xl" className={classes.root}>
                {props.children}
            </Container>
        );
    }
    return (
        <div id="page-content" className={classes.root}>
            {props.children}
        </div>
    );
}

export default PageContent;
