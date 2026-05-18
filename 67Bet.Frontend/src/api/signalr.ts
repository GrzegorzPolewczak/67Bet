import * as signalR from '@microsoft/signalr';

export interface LiveMatchState {
  matchId: string;
  sportKey: string;
  currentTime: string;
  currentAction: string;
  currentZone: string;
  momentum: number;
  score: Record<string, string>;
  statistics: Record<string, number>;
  timelineEvents: Array<{
    type: string;
    minute: string;
    description: string;
    team: string;
  }>;
}

const API_URL = import.meta.env.VITE_API_ODDS || 'http://localhost:5300';
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API_URL}/liveTrackerHub`)
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

export const startSignalRConnection = async () => {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    try {
      await connection.start();
      console.log('SignalR Connected.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      setTimeout(startSignalRConnection, 5000);
    }
  }
};

export const subscribeToMatch = async (matchId: string) => {
  if (connection.state === signalR.HubConnectionState.Connected) {
    try {
      await connection.invoke('SubscribeToMatch', matchId);
    } catch (err) {
      console.error('Error subscribing to match: ', err);
    }
  }
};

export const unsubscribeFromMatch = async (matchId: string) => {
  if (connection.state === signalR.HubConnectionState.Connected) {
    try {
      await connection.invoke('UnsubscribeFromMatch', matchId);
    } catch (err) {
      console.error('Error unsubscribing from match: ', err);
    }
  }
};

export const onMatchUpdate = (callback: (matchUpdate: LiveMatchState) => void) => {
  connection.on('ReceiveMatchUpdate', callback);
};

export const offMatchUpdate = () => {
  connection.off('ReceiveMatchUpdate');
};
