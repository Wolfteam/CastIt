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
            <div className={classes.root}>
                <Container id="page-content" maxWidth="xl">
                    {props.children}
                </Container>
            </div>
        );
    }
    return (
        <div id="page-content" className={classes.root}>
            {props.children}
        </div>
    );
}

export default PageContent;
