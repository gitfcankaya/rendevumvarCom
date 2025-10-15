import { useEffect, useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { authService } from '../services/authService';

interface AppointmentEvent {
  appointmentId: number;
  customerName?: string;
  serviceName?: string;
  startTime?: string;
  reason?: string;
  oldTime?: string;
  newTime?: string;
  oldStatus?: number;
  newStatus?: number;
}

interface UseAppointmentHubOptions {
  onAppointmentCreated?: (event: AppointmentEvent) => void;
  onAppointmentCancelled?: (event: AppointmentEvent) => void;
  onAppointmentRescheduled?: (event: AppointmentEvent) => void;
  onAppointmentStatusUpdated?: (event: AppointmentEvent) => void;
  onConnectionError?: (error: Error) => void;
}

export const useAppointmentHub = (options: UseAppointmentHubOptions = {}) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const reconnectAttempts = useRef(0);
  const maxReconnectAttempts = 5;
  const reconnectDelay = 3000; // 3 seconds

  const connect = useCallback(async () => {
    try {
      const token = authService.getToken();
      if (!token) {
        setError('No authentication token found');
        return;
      }

      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/hubs/appointments', {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with max 30 seconds
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers before connecting
      newConnection.on('AppointmentCreated', (event: AppointmentEvent) => {
        console.log('Appointment created:', event);
        if (options.onAppointmentCreated) {
          options.onAppointmentCreated(event);
        }
      });

      newConnection.on('AppointmentCancelled', (event: AppointmentEvent) => {
        console.log('Appointment cancelled:', event);
        if (options.onAppointmentCancelled) {
          options.onAppointmentCancelled(event);
        }
      });

      newConnection.on('AppointmentRescheduled', (event: AppointmentEvent) => {
        console.log('Appointment rescheduled:', event);
        if (options.onAppointmentRescheduled) {
          options.onAppointmentRescheduled(event);
        }
      });

      newConnection.on('AppointmentStatusUpdated', (event: AppointmentEvent) => {
        console.log('Appointment status updated:', event);
        if (options.onAppointmentStatusUpdated) {
          options.onAppointmentStatusUpdated(event);
        }
      });

      // Handle reconnection events
      newConnection.onreconnecting((error) => {
        console.log('SignalR reconnecting...', error);
        setIsConnected(false);
        setError('Connection lost. Reconnecting...');
      });

      newConnection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        setIsConnected(true);
        setError(null);
        reconnectAttempts.current = 0;
      });

      newConnection.onclose((error) => {
        console.log('SignalR connection closed', error);
        setIsConnected(false);
        
        if (error && reconnectAttempts.current < maxReconnectAttempts) {
          reconnectAttempts.current++;
          setError(`Connection closed. Attempting to reconnect (${reconnectAttempts.current}/${maxReconnectAttempts})...`);
          
          setTimeout(() => {
            connect();
          }, reconnectDelay);
        } else if (reconnectAttempts.current >= maxReconnectAttempts) {
          setError('Failed to reconnect after multiple attempts. Please refresh the page.');
          if (options.onConnectionError) {
            options.onConnectionError(new Error('Max reconnection attempts reached'));
          }
        }
      });

      await newConnection.start();
      console.log('SignalR Connected');
      setConnection(newConnection);
      setIsConnected(true);
      setError(null);
      reconnectAttempts.current = 0;

    } catch (err: any) {
      console.error('SignalR Connection Error:', err);
      setError(err.message || 'Failed to connect to SignalR hub');
      
      if (options.onConnectionError) {
        options.onConnectionError(err);
      }

      // Retry connection after delay
      if (reconnectAttempts.current < maxReconnectAttempts) {
        reconnectAttempts.current++;
        setTimeout(() => {
          connect();
        }, reconnectDelay);
      }
    }
  }, [options]);

  const disconnect = useCallback(async () => {
    if (connection) {
      try {
        await connection.stop();
        console.log('SignalR Disconnected');
      } catch (err) {
        console.error('Error disconnecting SignalR:', err);
      }
      setConnection(null);
      setIsConnected(false);
    }
  }, [connection]);

  // Connect on mount, disconnect on unmount
  useEffect(() => {
    connect();

    return () => {
      disconnect();
    };
  }, []); // Empty deps - only run on mount/unmount

  return {
    connection,
    isConnected,
    error,
    connect,
    disconnect,
  };
};

// Helper hook for showing toast notifications
export const useAppointmentNotifications = () => {
  const [notification, setNotification] = useState<{
    message: string;
    type: 'success' | 'info' | 'warning' | 'error';
    timestamp: number;
  } | null>(null);

  const showNotification = useCallback((message: string, type: 'success' | 'info' | 'warning' | 'error' = 'info') => {
    setNotification({
      message,
      type,
      timestamp: Date.now(),
    });

    // Auto-dismiss after 5 seconds
    setTimeout(() => {
      setNotification(null);
    }, 5000);
  }, []);

  const dismissNotification = useCallback(() => {
    setNotification(null);
  }, []);

  return {
    notification,
    showNotification,
    dismissNotification,
  };
};
