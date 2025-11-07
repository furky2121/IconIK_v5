import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { Toast } from 'primereact/toast';
import { Dropdown } from 'primereact/dropdown';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Avatar } from 'primereact/avatar';
import { Badge } from 'primereact/badge';
import { Panel } from 'primereact/panel';
import { Message } from 'primereact/message';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Toolbar } from 'primereact/toolbar';
import izinService from '../services/izinService';
import departmanService from '../services/departmanService';
import fileUploadService from '../services/fileUploadService';
import authService from '../services/authService';

const IzinTakvimi = () => {
    // Tarih parse utility - backend zaten doğru timezone ile gönderiyor
    const parseDate = (dateString) => {
        return new Date(dateString);
    };
    const [izinler, setIzinler] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [selectedDepartman, setSelectedDepartman] = useState(null);
    const [loading, setLoading] = useState(false);
    const [currentUser, setCurrentUser] = useState(null);
    const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth());
    const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
    const [calendarEvents, setCalendarEvents] = useState([]);
    const [detailDialog, setDetailDialog] = useState(false);
    const [selectedEvent, setSelectedEvent] = useState(null);
    const [viewMode, setViewMode] = useState('calendar'); // 'calendar' veya 'table'

    const toast = useRef(null);

    const months = [
        { label: 'Ocak', value: 0 },
        { label: 'Şubat', value: 1 },
        { label: 'Mart', value: 2 },
        { label: 'Nisan', value: 3 },
        { label: 'Mayıs', value: 4 },
        { label: 'Haziran', value: 5 },
        { label: 'Temmuz', value: 6 },
        { label: 'Ağustos', value: 7 },
        { label: 'Eylül', value: 8 },
        { label: 'Ekim', value: 9 },
        { label: 'Kasım', value: 10 },
        { label: 'Aralık', value: 11 }
    ];

    const years = [];
    for (let i = 2020; i <= 2030; i++) {
        years.push({ label: i.toString(), value: i });
    }

    useEffect(() => {
        const user = authService.getUser();
        setCurrentUser(user);
        loadData();
    }, [selectedDepartman, selectedMonth, selectedYear]);

    const loadData = async () => {
        await Promise.all([
            loadDepartmanlar(),
            loadIzinTakvimi()
        ]);
    };

    const loadDepartmanlar = async () => {
        try {
            // Filter dropdown için sadece aktif departmanları getir  
            const response = await departmanService.getAktifDepartmanlar();
            if (response.success) {
                setDepartmanlar(response.data);
            }
        } catch (error) {
            // console.error('Departmanlar yüklenirken hata:', error);
        }
    };

    const loadIzinTakvimi = async () => {
        setLoading(true);
        try {
            const user = authService.getUser();
            const kullaniciId = user?.id;

            if (!kullaniciId) {
                throw new Error('Kullanıcı bilgileri bulunamadı. Lütfen tekrar giriş yapın.');
            }

            // console.log('Loading izin takvimi with params:', { selectedDepartman, kullaniciId, userLevel: user.personel?.pozisyon?.kademe?.seviye });
            const response = await izinService.getIzinTakvimi(selectedDepartman, kullaniciId);
            // console.log('Izin takvimi response:', response);
            
            if (response.success) {
            // console.log('Raw calendar data:', response.data);
            // console.log('Selected month/year:', selectedMonth, selectedYear);
                
                // DEBUG: İlk kaydın tarih verisini kontrol et
                if (response.data.length > 0) {
                    const firstRecord = response.data[0];
            // console.log('DEBUG Frontend - İlk kayıt:', firstRecord);
            // console.log('DEBUG Frontend - Start:', firstRecord.start);
            // console.log('DEBUG Frontend - End:', firstRecord.end);
            // console.log('DEBUG Frontend - Parsed start date:', new Date(firstRecord.start));
            // console.log('DEBUG Frontend - Parsed end date:', new Date(firstRecord.end));
                }
                
                const filteredData = response.data.filter(event => {
                    const eventStart = new Date(event.start);
                    const eventEnd = new Date(event.end);
                    
            // console.log('Event dates:', eventStart, eventEnd, 'Event month:', eventStart.getMonth(), 'Event year:', eventStart.getFullYear());
                    
                    // İzin tarihi seçili ay/yılda mı kontrol et
                    return (eventStart.getMonth() === selectedMonth && eventStart.getFullYear() === selectedYear) ||
                           (eventEnd.getMonth() === selectedMonth && eventEnd.getFullYear() === selectedYear) ||
                           (eventStart <= new Date(selectedYear, selectedMonth + 1, 0) && eventEnd >= new Date(selectedYear, selectedMonth, 1));
                });

            // console.log('Filtered calendar data:', filteredData);
                // Onaylayan bilgisini kontrol et
                if (filteredData.length > 0) {
            // console.log('Sample data - onaylayanAd:', filteredData[0].onaylayanAd);
            // console.log('Sample data - durum:', filteredData[0].durum);
            // console.log('Full first record:', filteredData[0]);
                }
                setIzinler(filteredData);
                setCalendarEvents(filteredData);
            }
        } catch (error) {
            // console.error('İzin takvimi loading error:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'İzin takvimi yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const renderCalendarView = () => {
        const firstDay = new Date(selectedYear, selectedMonth, 1);
        const lastDay = new Date(selectedYear, selectedMonth + 1, 0);
        const startDate = new Date(firstDay);
        // Pazartesiden başlamak için: 0=Pazar, 1=Pazartesi, ... 6=Cumartesi
        const dayOfWeek = firstDay.getDay();
        const daysToSubtract = dayOfWeek === 0 ? 6 : dayOfWeek - 1; // Pazar ise 6 gün, diğer durumlarda (gün-1) gün çıkar
        startDate.setDate(startDate.getDate() - daysToSubtract);

        const weeks = [];
        const currentDate = new Date(startDate);

        while (weeks.length < 6) { // 6 hafta göster
            const week = [];
            for (let i = 0; i < 7; i++) {
                // Hafta sonu günlerinde (Cumartesi=5, Pazar=6) izin verilerini gösterme
                const isWeekend = (i === 5 || i === 6); // Cumartesi veya Pazar
                
                const dayEvents = isWeekend ? [] : izinler.filter(event => {
                    const eventStart = parseDate(event.start);
                    const eventEnd = parseDate(event.end);
                    
                    // Tarihleri sadece gün bazında karşılaştır (saat bilgisini göz ardı et)
                    const currentDayStart = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate());
                    const currentDayEnd = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() + 1);
                    const eventDayStart = new Date(eventStart.getFullYear(), eventStart.getMonth(), eventStart.getDate());
                    const eventDayEnd = new Date(eventEnd.getFullYear(), eventEnd.getMonth(), eventEnd.getDate());
                    
                    // İzin günleri: başlangıç günü dahil, işbaşı günü dahil değil
                    return currentDayStart >= eventDayStart && currentDayStart < eventDayEnd;
                });

                week.push({
                    date: new Date(currentDate),
                    events: dayEvents,
                    isCurrentMonth: currentDate.getMonth() === selectedMonth
                });
                currentDate.setDate(currentDate.getDate() + 1);
            }
            weeks.push(week);
        }

        return (
            <div className="calendar-view">
                <div className="calendar-header">
                    {['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar'].map((day, index) => (
                        <div key={day} className={`calendar-day-header ${(index === 5 || index === 6) ? 'weekend-header' : ''}`}>{day}</div>
                    ))}
                </div>
                <div className="calendar-body">
                    {weeks.map((week, weekIndex) => (
                        <div key={weekIndex} className="calendar-week">
                            {week.map((day, dayIndex) => {
                                const isWeekend = (dayIndex === 5 || dayIndex === 6); // Cumartesi veya Pazar
                                return (
                                <div 
                                    key={dayIndex} 
                                    className={`calendar-day ${!day.isCurrentMonth ? 'other-month' : ''} ${day.events.length > 0 ? 'has-events' : ''} ${isWeekend ? 'weekend' : ''}`}
                                >
                                    <div className="day-number">{day.date.getDate()}</div>
                                    <div className="day-events">
                                        {day.events.map(event => (
                                            event && event.personelAd ? (
                                                <div 
                                                    key={`${event.id}-${day.date.getTime()}`}
                                                    className="calendar-event-compact"
                                                    onClick={() => openEventDetail(event)}
                                                    title={`${event.personelAd} - ${event.personelPozisyon} - ${event.izinTipi} (${event.gunSayisi} gün)`}
                                                >
                                                    <div className="event-bar" style={{ backgroundColor: event.color }}></div>
                                                    <div className="event-text">
                                                        <div className="event-name">{event.personelAd}</div>
                                                        <div className="event-type">{event.izinTipi}</div>
                                                    </div>
                                                </div>
                                            ) : null
                                        ))}
                                    </div>
                                </div>
                                );
                            })}
                        </div>
                    ))}
                </div>
            </div>
        );
    };

    const openEventDetail = (event) => {
        setSelectedEvent(event);
        setDetailDialog(true);
    };

    const hideDetailDialog = () => {
        setDetailDialog(false);
        setSelectedEvent(null);
    };

    const adSoyadBodyTemplate = (rowData) => {
        if (!rowData || !rowData.personelAd) return null;
        
        const names = rowData.personelAd.split(' ');
        const avatarInitials = names[0].charAt(0) + (names[1] ? names[1].charAt(0) : '');
        
        return (
            <div className="flex align-items-center gap-2">
                <Avatar
                    label={avatarInitials}
                    size="normal"
                    shape="circle"
                    style={{ backgroundColor: rowData.color, color: '#ffffff' }}
                />
                <span>{rowData.personelAd}</span>
            </div>
        );
    };

    const getEventAvatar = (event) => {
        if (!event || !event.personelAd) return null;
        
        const names = event.personelAd.split(' ');
        return (
            <Avatar
                label={names[0].charAt(0) + (names[1] ? names[1].charAt(0) : '')}
                size="small"
                shape="circle"
                style={{ backgroundColor: event.color, color: '#ffffff', width: '20px', height: '20px', fontSize: '10px' }}
            />
        );
    };

    const izinTipiBodyTemplate = (rowData) => {
        return (
            <Badge
                value={rowData.izinTipi}
                severity={izinService.getIzinTipiRengi(rowData.izinTipi)}
            />
        );
    };

    const durumBodyTemplate = (rowData) => {
        // İzin takviminde sadece onaylı izinler gösterilir
        // Ama eğer durum bilgisi varsa onu göster
        const durum = rowData.durum || 'Onaylandı';
        let severity = 'success';
        
        if (durum === 'Bekliyor') severity = 'warning';
        else if (durum === 'Reddedildi') severity = 'danger';
        else if (durum === 'İptal Edildi') severity = 'secondary';
        
        return (
            <Badge
                value={durum}
                severity={severity}
            />
        );
    };

    const tarihBodyTemplate = (field) => (rowData) => {
        const date = new Date(rowData[field]);
        return date.toLocaleDateString('tr-TR') + ' ' + date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    };

    const izinBaslangicBodyTemplate = (rowData) => {
        const date = parseDate(rowData.start);
        return date.toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const isbasiBodyTemplate = (rowData) => {
        const date = parseDate(rowData.end);
        return date.toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                <div className="p-d-flex p-ai-center">
                    <Dropdown
                        value={selectedMonth}
                        options={months}
                        onChange={(e) => setSelectedMonth(e.value)}
                        className="p-mr-2"
                        placeholder="Ay seçiniz"
                    />
                    <Dropdown
                        value={selectedYear}
                        options={years}
                        onChange={(e) => setSelectedYear(e.value)}
                        className="p-mr-2"
                        placeholder="Yıl seçiniz"
                    />
                    <Dropdown
                        value={selectedDepartman}
                        options={departmanlar}
                        optionLabel="ad"
                        optionValue="id"
                        onChange={(e) => setSelectedDepartman(e.value)}
                        placeholder="Tüm Departmanlar"
                        showClear
                        className="p-mr-2"
                    />
                </div>
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <div className="p-d-flex p-ai-center">
                    <Button
                        label="Takvim"
                        icon="pi pi-calendar"
                        className={`p-button-sm p-mr-1 ${viewMode === 'calendar' ? '' : 'p-button-outlined'}`}
                        onClick={() => setViewMode('calendar')}
                    />
                    <Button
                        label="Liste"
                        icon="pi pi-list"
                        className={`p-button-sm ${viewMode === 'table' ? '' : 'p-button-outlined'}`}
                        onClick={() => setViewMode('table')}
                    />
                </div>
            </React.Fragment>
        );
    };

    const getYetkiMesaji = () => {
        if (!currentUser || !currentUser.personel?.pozisyon?.kademe) {
            return "Yetki seviyeniz belirlenemiyor.";
        }

        const kademeSeviye = currentUser.personel.pozisyon.kademe.seviye;
        const kademeAd = currentUser.personel.pozisyon.kademe.ad;
        
        if (kademeSeviye === 1 || kademeSeviye === 2) {
            // Genel Müdür (1), Direktör (2)
            return `${kademeAd} - Tüm personellerin onaylanmış izinlerini görüntüleyebilirsiniz.`;
        } else if (kademeSeviye >= 3 && kademeSeviye <= 6) {
            // Grup Müdürü (3), Müdür (4), Yönetici (5), Sorumlu (6)
            return `${kademeAd} - Kendinizin ve yetki alanınızdaki personellerin onaylanmış izinlerini görüntüleyebilirsiniz.`;
        } else {
            // Kıdemli Uzman (7), Uzman (8), Uzman Yardımcısı (9)
            return `${kademeAd} - Sadece kendi onaylanmış izinlerinizi görüntüleyebilirsiniz.`;
        }
    };

    return (
        <div className="izin-takvimi">
            <Toast ref={toast} />

            {/* CSS Styles */}
            <style>
                {`
                    .calendar-view {
                        border: 1px solid #e3e3e3;
                        border-radius: 6px;
                        overflow: hidden;
                    }
                    .calendar-header {
                        display: grid;
                        grid-template-columns: repeat(7, 1fr);
                        background-color: #f8f9fa;
                        border-bottom: 1px solid #e3e3e3;
                    }
                    .calendar-day-header {
                        padding: 12px 8px;
                        text-align: center;
                        font-weight: 600;
                        border-right: 1px solid #e3e3e3;
                    }
                    .calendar-day-header:last-child {
                        border-right: none;
                    }
                    .calendar-day-header.weekend-header {
                        background-color: #f3f3f3;
                        color: #9ca3af;
                    }
                    .calendar-body {
                        display: flex;
                        flex-direction: column;
                    }
                    .calendar-week {
                        display: grid;
                        grid-template-columns: repeat(7, 1fr);
                        border-bottom: 1px solid #e3e3e3;
                    }
                    .calendar-week:last-child {
                        border-bottom: none;
                    }
                    .calendar-day {
                        min-height: 100px;
                        padding: 8px;
                        border-right: 1px solid #e3e3e3;
                        background-color: #ffffff;
                    }
                    .calendar-day:last-child {
                        border-right: none;
                    }
                    .calendar-day.other-month {
                        background-color: #f8f9fa;
                        color: #6c757d;
                    }
                    .calendar-day.has-events {
                        background-color: #f8f9ff;
                    }
                    .calendar-day.weekend {
                        background-color: #f8f8f8;
                        color: #9ca3af;
                    }
                    .calendar-day.weekend .day-number {
                        color: #9ca3af;
                    }
                    .day-number {
                        font-weight: 600;
                        margin-bottom: 4px;
                    }
                    .day-events {
                        display: flex;
                        flex-direction: column;
                        gap: 2px;
                    }
                    .calendar-event-compact {
                        display: flex;
                        align-items: center;
                        margin-bottom: 1px;
                        cursor: pointer;
                        border-radius: 3px;
                        background: #ffffff;
                        border: 1px solid #e9ecef;
                        padding: 2px 4px;
                        transition: all 0.2s ease;
                        box-shadow: 0 1px 2px rgba(0,0,0,0.05);
                        min-height: 18px;
                        max-height: 18px;
                        overflow: hidden;
                    }
                    .calendar-event-compact:hover {
                        transform: translateY(-1px);
                        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                        border-color: #6366f1;
                        z-index: 10;
                        position: relative;
                    }
                    .event-bar {
                        width: 3px;
                        height: 14px;
                        border-radius: 1px;
                        margin-right: 4px;
                        flex-shrink: 0;
                    }
                    .event-text {
                        flex: 1;
                        min-width: 0;
                        display: flex;
                        flex-direction: column;
                        justify-content: center;
                        overflow: hidden;
                    }
                    .event-name {
                        font-weight: 600;
                        font-size: 9px;
                        color: #1f2937;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        white-space: nowrap;
                        line-height: 1.1;
                        margin-bottom: 1px;
                    }
                    .event-type {
                        font-size: 8px;
                        color: #6b7280;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        white-space: nowrap;
                        line-height: 1.1;
                    }
                    .event-more-modern {
                        font-size: 9px;
                        color: #6b7280;
                        text-align: center;
                        margin-top: 2px;
                        padding: 1px 3px;
                        background: #f3f4f6;
                        border-radius: 2px;
                    }
                `}
            </style>

            <Card>
                {/* Yetki Durumu Bilgisi */}
                {currentUser && (
                    <div className="p-mb-3">
                        <Message
                            severity="info"
                            text={getYetkiMesaji()}
                            className="p-mb-2"
                        />
                    </div>
                )}
                
                <Toolbar
                    className="p-mb-4"
                    left={leftToolbarTemplate}
                    right={rightToolbarTemplate}
                />

                {viewMode === 'calendar' ? renderCalendarView() : (
                    <DataTable
                        value={izinler || []}
                        loading={loading}
                        dataKey="id"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        emptyMessage="İzin bulunamadı."
                        responsiveLayout="scroll"
                        filters={null}
                    >
                        <Column
                            field="id"
                            header="ID"
                            sortable
                            style={{ width: '80px', textAlign: 'center' }}
                        />
                        <Column
                            header="Ad Soyad"
                            body={adSoyadBodyTemplate}
                            sortable
                            style={{ minWidth: '14rem' }}
                            sortField="personelAd"
                        />
                        <Column
                            field="personelPozisyon"
                            header="Pozisyon"
                            sortable
                            style={{ minWidth: '10rem' }}
                        />
                        <Column
                            field="personelDepartman"
                            header="Departman"
                            sortable
                            style={{ minWidth: '10rem' }}
                        />
                        <Column
                            field="izinTipi"
                            header="İzin Tipi"
                            body={izinTipiBodyTemplate}
                            sortable
                            style={{ minWidth: '10rem' }}
                        />
                        <Column
                            field="durum"
                            header="Durum"
                            body={durumBodyTemplate}
                            sortable
                            style={{ minWidth: '8rem' }}
                        />
                        <Column
                            field="onaylayanAd"
                            header="Onaylayan"
                            sortable
                            style={{ minWidth: '10rem' }}
                        />
                        <Column
                            field="start"
                            header="İzin Başlangıç"
                            body={izinBaslangicBodyTemplate}
                            sortable
                            style={{ minWidth: '12rem' }}
                        />
                        <Column
                            field="end"
                            header="İşbaşı"
                            body={isbasiBodyTemplate}
                            sortable
                            style={{ minWidth: '12rem' }}
                        />
                        <Column
                            field="gunSayisi"
                            header="Gün"
                            sortable
                            style={{ minWidth: '6rem' }}
                        />
                        <Column
                            field="aciklama"
                            header="İzin Sebebi"
                            sortable
                            style={{ minWidth: '12rem' }}
                        />
                    </DataTable>
                )}
            </Card>

            {/* Event Detail Dialog */}
            <Dialog
                visible={detailDialog}
                style={{ width: '600px' }}
                header={
                    <div className="flex align-items-center gap-3">
                        <i className="pi pi-calendar text-primary" style={{ fontSize: '1.5rem' }}></i>
                        <span>İzin Detayları</span>
                    </div>
                }
                modal
                onHide={hideDetailDialog}
                className="p-fluid"
            >
                {selectedEvent && (
                    <div className="grid">
                        <div className="col-12">
                            <div className="surface-50 p-4 border-round mb-4">
                                <div className="flex align-items-center gap-3">
                                    {getEventAvatar(selectedEvent)}
                                    <div className="flex-1">
                                        <div className="text-xl font-semibold text-900 mb-1">
                                            {selectedEvent.personelAd}
                                        </div>
                                        <div className="text-600 text-sm">
                                            <i className="pi pi-briefcase mr-1"></i>
                                            {selectedEvent.personelPozisyon || 'Pozisyon bilgisi yok'}
                                        </div>
                                        <div className="text-600 text-sm">
                                            <i className="pi pi-building mr-1"></i>
                                            {selectedEvent.personelDepartman}
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <Badge
                                            value={selectedEvent.izinTipi}
                                            severity={izinService.getIzinTipiRengi(selectedEvent.izinTipi)}
                                            size="large"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="col-12">
                            <div className="surface-100 p-3 border-round mb-3">
                                <div className="flex align-items-center justify-content-center gap-2">
                                    <i className="pi pi-clock text-primary"></i>
                                    <span className="font-semibold text-lg text-900">
                                        {selectedEvent.gunSayisi} İş Günü
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="col-6">
                            <div className="surface-card border-1 border-200 p-3 border-round">
                                <div className="flex align-items-center gap-2 mb-2">
                                    <i className="pi pi-play-circle text-green-600"></i>
                                    <span className="text-sm text-600 font-medium">İzin Başlangıç</span>
                                </div>
                                <div className="font-semibold text-900">
                                    {parseDate(selectedEvent.start).toLocaleString('tr-TR', {
                                        weekday: 'long',
                                        day: '2-digit',
                                        month: '2-digit',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}
                                </div>
                            </div>
                        </div>

                        <div className="col-6">
                            <div className="surface-card border-1 border-200 p-3 border-round">
                                <div className="flex align-items-center gap-2 mb-2">
                                    <i className="pi pi-stop-circle text-orange-600"></i>
                                    <span className="text-sm text-600 font-medium">İşbaşı</span>
                                </div>
                                <div className="font-semibold text-900">
                                    {parseDate(selectedEvent.end).toLocaleString('tr-TR', {
                                        weekday: 'long',
                                        day: '2-digit',
                                        month: '2-digit',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}
                                </div>
                            </div>
                        </div>

                        {selectedEvent.onaylayanAd && (
                            <div className="col-12">
                                <div className="surface-card border-1 border-200 p-3 border-round">
                                    <div className="flex align-items-center gap-2 mb-2">
                                        <i className="pi pi-check-circle text-green-600"></i>
                                        <span className="text-sm text-600 font-medium">Onaylayan</span>
                                    </div>
                                    <div className="font-semibold text-900">
                                        {selectedEvent.onaylayanAd}
                                    </div>
                                </div>
                            </div>
                        )}

                        {selectedEvent.aciklama && (
                            <div className="col-12">
                                <div className="surface-card border-1 border-200 p-3 border-round">
                                    <div className="flex align-items-center gap-2 mb-2">
                                        <i className="pi pi-comment text-blue-600"></i>
                                        <span className="text-sm text-600 font-medium">İzin Açıklaması</span>
                                    </div>
                                    <div className="text-900 line-height-3">
                                        {selectedEvent.aciklama}
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default IzinTakvimi;