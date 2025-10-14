import React, { useState } from 'react';
import type { Appointment } from '../services/appointmentService';

interface AppointmentCalendarProps {
  appointments: Appointment[];
  selectedDate: Date;
  onDateChange: (date: Date) => void;
  view: 'daily' | 'weekly' | 'monthly';
}

export const AppointmentCalendar: React.FC<AppointmentCalendarProps> = ({
  appointments,
  selectedDate,
  onDateChange,
  view
}) => {
  const [currentMonth, setCurrentMonth] = useState(selectedDate.getMonth());
  const [currentYear, setCurrentYear] = useState(selectedDate.getFullYear());

  const months = [
    'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
    'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
  ];

  const days = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];

  const getDaysInMonth = (month: number, year: number) => {
    return new Date(year, month + 1, 0).getDate();
  };

  const getFirstDayOfMonth = (month: number, year: number) => {
    const firstDay = new Date(year, month, 1).getDay();
    return firstDay === 0 ? 6 : firstDay - 1; // Pazartesi = 0
  };

  const getAppointmentCountForDate = (date: Date) => {
    const dateStr = date.toISOString().split('T')[0];
    return appointments.filter(apt => apt.appointmentDate === dateStr).length;
  };

  const renderCalendarDays = () => {
    const daysInMonth = getDaysInMonth(currentMonth, currentYear);
    const firstDay = getFirstDayOfMonth(currentMonth, currentYear);
    const days = [];

    // Önceki ayın son günleri
    for (let i = firstDay - 1; i >= 0; i--) {
      const prevMonth = currentMonth === 0 ? 11 : currentMonth - 1;
      const prevYear = currentMonth === 0 ? currentYear - 1 : currentYear;
      const daysInPrevMonth = getDaysInMonth(prevMonth, prevYear);
      const day = daysInPrevMonth - i;
      
      days.push(
        <div key={`prev-${day}`} className="calendar-day other-month">
          <span className="day-number">{day}</span>
        </div>
      );
    }

    // Bu ayın günleri
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(currentYear, currentMonth, day);
      const isToday = date.toDateString() === new Date().toDateString();
      const isSelected = date.toDateString() === selectedDate.toDateString();
      const appointmentCount = getAppointmentCountForDate(date);

      days.push(
        <div
          key={day}
          className={`calendar-day ${isToday ? 'today' : ''} ${isSelected ? 'selected' : ''} ${appointmentCount > 0 ? 'has-appointments' : ''}`}
          onClick={() => onDateChange(date)}
        >
          <span className="day-number">{day}</span>
          {appointmentCount > 0 && (
            <span className="appointment-count">{appointmentCount}</span>
          )}
        </div>
      );
    }

    // Sonraki ayın ilk günleri
    const totalCells = Math.ceil((firstDay + daysInMonth) / 7) * 7;
    const remainingCells = totalCells - (firstDay + daysInMonth);
    
    for (let day = 1; day <= remainingCells; day++) {
      days.push(
        <div key={`next-${day}`} className="calendar-day other-month">
          <span className="day-number">{day}</span>
        </div>
      );
    }

    return days;
  };

  const navigateMonth = (direction: 'prev' | 'next') => {
    if (direction === 'prev') {
      if (currentMonth === 0) {
        setCurrentMonth(11);
        setCurrentYear(currentYear - 1);
      } else {
        setCurrentMonth(currentMonth - 1);
      }
    } else {
      if (currentMonth === 11) {
        setCurrentMonth(0);
        setCurrentYear(currentYear + 1);
      } else {
        setCurrentMonth(currentMonth + 1);
      }
    }
  };

  if (view === 'daily') {
    return (
      <div className="appointment-calendar daily-view">
        <div className="calendar-header">
          <h3>{selectedDate.toLocaleDateString('tr-TR', { 
            weekday: 'long', 
            year: 'numeric', 
            month: 'long', 
            day: 'numeric' 
          })}</h3>
          <div className="date-navigation">
            <button 
              onClick={() => onDateChange(new Date(selectedDate.getTime() - 24 * 60 * 60 * 1000))}
              className="nav-button"
            >
              ←
            </button>
            <button 
              onClick={() => onDateChange(new Date())}
              className="today-button"
            >
              Bugün
            </button>
            <button 
              onClick={() => onDateChange(new Date(selectedDate.getTime() + 24 * 60 * 60 * 1000))}
              className="nav-button"
            >
              →
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (view === 'weekly') {
    const startOfWeek = new Date(selectedDate);
    const day = startOfWeek.getDay();
    const diff = startOfWeek.getDate() - day + (day === 0 ? -6 : 1);
    startOfWeek.setDate(diff);

    const weekDays = [];
    for (let i = 0; i < 7; i++) {
      const day = new Date(startOfWeek);
      day.setDate(startOfWeek.getDate() + i);
      weekDays.push(day);
    }

    return (
      <div className="appointment-calendar weekly-view">
        <div className="calendar-header">
          <h3>
            {startOfWeek.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })} - 
            {weekDays[6].toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric' })}
          </h3>
          <div className="date-navigation">
            <button 
              onClick={() => {
                const newDate = new Date(selectedDate);
                newDate.setDate(selectedDate.getDate() - 7);
                onDateChange(newDate);
              }}
              className="nav-button"
            >
              ←
            </button>
            <button 
              onClick={() => onDateChange(new Date())}
              className="today-button"
            >
              Bu Hafta
            </button>
            <button 
              onClick={() => {
                const newDate = new Date(selectedDate);
                newDate.setDate(selectedDate.getDate() + 7);
                onDateChange(newDate);
              }}
              className="nav-button"
            >
              →
            </button>
          </div>
        </div>
        <div className="week-grid">
          {weekDays.map((day, index) => {
            const isToday = day.toDateString() === new Date().toDateString();
            const appointmentCount = getAppointmentCountForDate(day);
            return (
              <div
                key={index}
                className={`week-day ${isToday ? 'today' : ''} ${appointmentCount > 0 ? 'has-appointments' : ''}`}
                onClick={() => onDateChange(day)}
              >
                <div className="day-header">
                  <span className="day-name">{days[index]}</span>
                  <span className="day-number">{day.getDate()}</span>
                </div>
                {appointmentCount > 0 && (
                  <div className="appointment-indicator">
                    {appointmentCount} randevu
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    );
  }

  return (
    <div className="appointment-calendar monthly-view">
      <div className="calendar-header">
        <h3>{months[currentMonth]} {currentYear}</h3>
        <div className="month-navigation">
          <button onClick={() => navigateMonth('prev')} className="nav-button">
            ←
          </button>
          <button 
            onClick={() => {
              const today = new Date();
              setCurrentMonth(today.getMonth());
              setCurrentYear(today.getFullYear());
              onDateChange(today);
            }}
            className="today-button"
          >
            Bu Ay
          </button>
          <button onClick={() => navigateMonth('next')} className="nav-button">
            →
          </button>
        </div>
      </div>
      
      <div className="calendar-grid">
        <div className="calendar-days-header">
          {days.map(day => (
            <div key={day} className="day-header">{day}</div>
          ))}
        </div>
        <div className="calendar-days">
          {renderCalendarDays()}
        </div>
      </div>
    </div>
  );
};