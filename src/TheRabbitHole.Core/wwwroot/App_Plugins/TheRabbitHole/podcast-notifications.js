import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import * as signalR from 'https://esm.sh/@microsoft/signalr@8.0.7';

let connection;

export const onInit = async (host) => {
    const notifications = await host.getContext(UMB_NOTIFICATION_CONTEXT);

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/umbraco/backoffice/hubs/podcast')
        .withAutomaticReconnect()
        .build();

    connection.on('episodeProcessed', (contentKey, name) => {
        notifications?.peek('positive', {
            data: {
                headline: 'Podcast episode processed',
                message: `${name ?? contentKey} — transcript and show notes are ready.`
            }
        });
    });

    try {
        await connection.start();
    } catch (err) {
        console.warn('[TheRabbitHole] Could not connect to podcast hub', err);
    }
};

export const onUnload = async () => {
    await connection?.stop();
};
