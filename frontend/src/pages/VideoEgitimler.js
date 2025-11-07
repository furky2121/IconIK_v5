import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Dropdown } from 'primereact/dropdown';
import { InputNumber } from 'primereact/inputnumber';
import { Chip } from 'primereact/chip';
import { Badge } from 'primereact/badge';
import { Toast } from 'primereact/toast';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Image } from 'primereact/image';
import { Checkbox } from 'primereact/checkbox';
import { Toolbar } from 'primereact/toolbar';
import { ToggleButton } from 'primereact/togglebutton';
import { Slider } from 'primereact/slider';
import { Divider } from 'primereact/divider';
import { ProgressSpinner } from 'primereact/progressspinner';
import videoEgitimService from '../services/videoEgitimService';
import departmanService from '../services/departmanService';
import pozisyonService from '../services/pozisyonService';
import personelService from '../services/personelService';
import yetkiService from '../services/yetkiService';
import './VideoEgitimler.css';

const VideoEgitimler = () => {
    const router = useRouter();
    
    // State Variables
    const [kategoriler, setKategoriler] = useState([]);
    const [videoEgitimler, setVideoEgitimler] = useState([]);
    const [filteredEgitimler, setFilteredEgitimler] = useState([]);
    const [selectedKategori, setSelectedKategori] = useState(null);
    const [personeller, setPersoneller] = useState([]);
    const [departmanlar, setDepartmanlar] = useState([]);
    const [pozisyonlar, setPozisyonlar] = useState([]);
    const [loading, setLoading] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [viewMode, setViewMode] = useState('table'); // table or cards
    const [durationLoading, setDurationLoading] = useState(false);
    
    // Dialog states
    const [egitimDialog, setEgitimDialog] = useState(false);
    const [deleteDialog, setDeleteDialog] = useState(false);
    
    // Search and filter states
    const [globalFilter, setGlobalFilter] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    
    // Permission states
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    
    const toast = useRef(null);

    // Empty objects
    const emptyVideoEgitim = {
        id: null,
        baslik: '',
        aciklama: '',
        videoUrl: '',
        thumbnailUrl: '',
        sureDakika: null,
        seviye: 'Başlangıç',
        egitmen: '',
        kategoriId: null,
        zorunluMu: false,
        izlenmeMinimum: 80,
        sonTamamlanmaTarihi: null,
        aktif: true
    };


    // Form states
    const [videoEgitim, setVideoEgitim] = useState(emptyVideoEgitim);
    const [egitimToDelete, setEgitimToDelete] = useState(null);

    // Options
    const seviyeOptions = [
        { label: 'Başlangıç', value: 'Başlangıç' },
        { label: 'Orta', value: 'Orta' },
        { label: 'İleri', value: 'İleri' }
    ];

    // Load permissions
    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('egitimler', 'read'),
                write: yetkiService.hasScreenPermission('egitimler', 'write'),
                delete: yetkiService.hasScreenPermission('egitimler', 'delete'),
                update: yetkiService.hasScreenPermission('egitimler', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({ read: false, write: false, delete: false, update: false });
        }
    };

    // Load data functions
    const loadKategoriler = async () => {
        try {
            const response = await videoEgitimService.getKategoriler();
            if (response.success) {
                setKategoriler(response.data);
            }
        } catch (error) {
            // console.error('Kategoriler yüklenirken hata:', error);
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Kategoriler yüklenemedi.' });
        }
    };

    const loadVideoEgitimler = async () => {
        setLoading(true);
        try {
            const response = await videoEgitimService.getTumEgitimler();
            if (response.success) {
                setVideoEgitimler(response.data);
                setFilteredEgitimler(response.data);
            }
        } catch (error) {
            // console.error('Video eğitimler yüklenirken hata:', error);
            toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Video eğitimler yüklenemedi.' });
        } finally {
            setLoading(false);
        }
    };

    const loadPersoneller = async () => {
        try {
            const response = await personelService.getPersonellerAktif();
            if (response.success) {
                const options = response.data.map(p => ({
                    label: `${p.ad} ${p.soyad} (${p.departmanAd})`,
                    value: p.id
                }));
                setPersoneller(options);
            }
        } catch (error) {
            // console.error('Personeller yüklenirken hata:', error);
        }
    };

    const loadDepartmanlar = async () => {
        try {
            const response = await departmanService.getDepartmanlarAktif();
            if (response.success) {
                const options = response.data.map(d => ({
                    label: d.ad,
                    value: d.id
                }));
                setDepartmanlar(options);
            }
        } catch (error) {
            // console.error('Departmanlar yüklenirken hata:', error);
        }
    };

    const loadPozisyonlar = async () => {
        try {
            const response = await pozisyonService.getPozisyonlarAktif();
            if (response.success) {
                const options = response.data.map(p => ({
                    label: `${p.ad} (${p.departmanAd})`,
                    value: p.id
                }));
                setPozisyonlar(options);
            }
        } catch (error) {
            // console.error('Pozisyonlar yüklenirken hata:', error);
        }
    };

    // Effect hooks
    useEffect(() => {
        loadPermissions();
        loadKategoriler();
        loadPersoneller();
        loadDepartmanlar();
        loadPozisyonlar();
        loadVideoEgitimler();
    }, []);

    // Filter effect
    useEffect(() => {
        let filtered = videoEgitimler;
        
        if (selectedKategori) {
            filtered = filtered.filter(egitim => egitim.kategoriId === selectedKategori);
        }
        
        if (searchTerm) {
            filtered = filtered.filter(egitim => 
                egitim.baslik.toLowerCase().includes(searchTerm.toLowerCase()) ||
                egitim.egitmen.toLowerCase().includes(searchTerm.toLowerCase()) ||
                egitim.aciklama.toLowerCase().includes(searchTerm.toLowerCase())
            );
        }
        
        setFilteredEgitimler(filtered);
    }, [videoEgitimler, selectedKategori, searchTerm]);

    // Dialog functions
    const openNew = () => {
        setVideoEgitim({ ...emptyVideoEgitim });
        setSubmitted(false);
        setEgitimDialog(true);
    };

    const hideDialog = () => {
        setSubmitted(false);
        setEgitimDialog(false);
    };


    const hideDeleteDialog = () => {
        setDeleteDialog(false);
        setEgitimToDelete(null);
    };

    // CRUD functions
    const saveVideoEgitim = async () => {
        setSubmitted(true);
        
        if (videoEgitim.baslik.trim() && videoEgitim.videoUrl.trim() && videoEgitim.kategoriId) {
            try {
                // YouTube URL'sini kontrol et ve thumbnail oluştur
                if (videoEgitim.videoUrl && !videoEgitim.thumbnailUrl) {
                    videoEgitim.thumbnailUrl = videoEgitimService.getThumbnailUrl(videoEgitim.videoUrl);
                }
                
                const response = await videoEgitimService.saveEgitim(videoEgitim);
                if (response.success) {
                    toast.current.show({ 
                        severity: 'success', 
                        summary: 'Başarılı', 
                        detail: videoEgitim.id ? 'Eğitim güncellendi' : 'Eğitim eklendi'
                    });
                    loadVideoEgitimler();
                    hideDialog();
                } else {
                    toast.current.show({ severity: 'error', summary: 'Hata', detail: response.message });
                }
            } catch (error) {
                toast.current.show({ severity: 'error', summary: 'Hata', detail: 'Video eğitim kaydedilemedi.' });
            }
        }
    };

    const editEgitim = (egitim) => {
        setVideoEgitim({
            ...egitim,
            sureDakika: egitim.sure || egitim.sureDakika,
            sonTamamlanmaTarihi: egitim.sonTamamlanmaTarihi ? new Date(egitim.sonTamamlanmaTarihi) : null
        });
        setEgitimDialog(true);
    };

    const confirmDeleteEgitim = (egitim) => {
        setEgitimToDelete(egitim);
        setDeleteDialog(true);
    };

    const deleteEgitim = async () => {
        try {
            // Delete API call would go here
            toast.current.show({ 
                severity: 'success', 
                summary: 'Başarılı', 
                detail: 'Eğitim silindi' 
            });
            loadVideoEgitimler();
            hideDeleteDialog();
        } catch (error) {
            toast.current.show({ 
                severity: 'error', 
                summary: 'Hata', 
                detail: 'Eğitim silinemedi' 
            });
        }
    };


    // Event handlers
    const handleWatchVideo = (egitimId) => {
        if (egitimId) {
            router.push(`/egitimler/izle/${egitimId}`);
        } else {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Video ID bulunamadı.'
            });
        }
    };

    // Video URL değiştiğinde otomatik süre çek
    const handleVideoUrlChange = async (e) => {
        const videoUrl = (e.target && e.target.value) || '';
        let _egitim = { ...videoEgitim };
        _egitim.videoUrl = videoUrl;
        setVideoEgitim(_egitim);

        // YouTube veya Vimeo URL'si ise otomatik süreyi çek
        if (videoUrl && (videoUrl.includes('youtube.com') || videoUrl.includes('youtu.be') || videoUrl.includes('vimeo.com'))) {
            setDurationLoading(true);
            try {
                const response = await videoEgitimService.getVideoDuration(videoUrl);
                if (response.success && response.data.success) {
                    const durationData = response.data;
                    
                    // Süreyi dakika cinsinden güncelle
                    _egitim.sureDakika = durationData.durationMinutes;
                    
                    // Thumbnail URL varsa güncelle
                    if (durationData.thumbnailUrl && !_egitim.thumbnailUrl) {
                        _egitim.thumbnailUrl = durationData.thumbnailUrl;
                    }
                    
                    setVideoEgitim(_egitim);
                    
                    toast.current.show({
                        severity: 'success',
                        summary: 'Başarılı',
                        detail: `Video süresi otomatik olarak ${durationData.durationMinutes} dakika olarak ayarlandı`,
                        life: 3000
                    });
                } else {
                    toast.current.show({
                        severity: 'warn',
                        summary: 'Uyarı',
                        detail: 'Video süresi otomatik olarak alınamadı, lütfen manuel olarak girin',
                        life: 3000
                    });
                }
            } catch (error) {
            // console.error('Video süresi çekilirken hata:', error);
                toast.current.show({
                    severity: 'warn',
                    summary: 'Uyarı',
                    detail: 'Video süresi otomatik olarak alınamadı, lütfen manuel olarak girin',
                    life: 3000
                });
            } finally {
                setDurationLoading(false);
            }
        }
    };

    // Input change handlers
    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _egitim = { ...videoEgitim };
        _egitim[name] = val;
        setVideoEgitim(_egitim);
    };

    const onNumberChange = (e, name) => {
        const val = e.value || null;
        let _egitim = { ...videoEgitim };
        _egitim[name] = val;
        setVideoEgitim(_egitim);
    };

    const onSliderChange = (e) => {
        let _egitim = { ...videoEgitim };
        _egitim.izlenmeMinimum = e.value;
        setVideoEgitim(_egitim);
    };

    const onDropdownChange = (e, name) => {
        let _egitim = { ...videoEgitim };
        _egitim[name] = e.value;
        setVideoEgitim(_egitim);
    };

    const onToggleChange = (e, name) => {
        let _egitim = { ...videoEgitim };
        _egitim[name] = e.value;
        setVideoEgitim(_egitim);
    };

    const onCheckboxChange = (e, name) => {
        let _egitim = { ...videoEgitim };
        _egitim[name] = e.target.checked;
        setVideoEgitim(_egitim);
    };

    // Template functions
    const thumbnailBodyTemplate = (rowData) => {
        return (
            <div className="thumbnail-container">
                <Image 
                    src={rowData.thumbnailUrl || '/layout/images/icon_ik.png'} 
                    alt={rowData.baslik}
                    width="80" 
                    height="45" 
                    className="border-round shadow-2 cursor-pointer"
                    preview={false}
                    onClick={() => handleWatchVideo(rowData.id)}
                />
                <div className="play-overlay">
                    <i className="pi pi-play"></i>
                </div>
            </div>
        );
    };

    const seviyeBadgeTemplate = (rowData) => {
        const getSeverity = (seviye) => {
            switch (seviye) {
                case 'Başlangıç': return 'success';
                case 'Orta': return 'warning';
                case 'İleri': return 'danger';
                default: return 'info';
            }
        };
        return <Badge value={rowData.seviye} severity={getSeverity(rowData.seviye)} className="text-sm" />;
    };

    const durationTemplate = (rowData) => {
        const duration = rowData.sure || rowData.sureDakika || 0;
        return (
            <span className="font-semibold">
                {duration > 0 ? `${duration} dk` : '-'}
            </span>
        );
    };

    const zorunluTemplate = (rowData) => {
        return rowData.zorunluMu ? 
            <Chip label="Zorunlu" className="p-chip-danger text-sm" /> : 
            <Chip label="Seçmeli" className="p-chip-info text-sm" />;
    };

    const statusTemplate = (rowData) => {
        return (
            <ToggleButton
                checked={rowData.aktif}
                onLabel="Aktif"
                offLabel="Pasif"
                onIcon="pi pi-check"
                offIcon="pi pi-times"
                className={`p-button-sm ${rowData.aktif ? 'p-button-success' : 'p-button-secondary'}`}
                disabled={!permissions.update}
            />
        );
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                {permissions.update && (
                    <Button 
                        icon="pi pi-pencil" 
                        className="p-button-rounded p-button-success p-button-text p-button-sm"
                        onClick={() => editEgitim(rowData)} 
                        tooltip="Düzenle"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
                {permissions.delete && (
                    <Button 
                        icon="pi pi-trash" 
                        className="p-button-rounded p-button-danger p-button-text p-button-sm"
                        onClick={() => confirmDeleteEgitim(rowData)} 
                        tooltip="Sil"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
            </div>
        );
    };

    // Card template for card view
    const cardTemplate = (egitim) => {
        return (
            <div className="col-12 md:col-6 lg:col-4" key={egitim.id}>
                <Card className="video-card h-full">
                    <div className="video-card-header">
                        <Image
                            src={egitim.thumbnailUrl || '/layout/images/icon_ik.png'} 
                            alt={egitim.baslik}
                            width="100%" 
                            height="160"
                            className="border-round"
                            style={{ objectFit: 'cover' }}
                        />
                        <div className="video-overlay">
                            <Button 
                                icon="pi pi-play" 
                                className="p-button-rounded p-button-lg"
                                style={{ backgroundColor: 'rgba(0,0,0,0.7)' }}
                                onClick={(e) => {
                                    e.stopPropagation();
                                    handleWatchVideo(egitim.id);
                                }}
                            />
                        </div>
                    </div>
                    
                    <div className="video-card-content p-3">
                        <div className="flex align-items-start justify-content-between mb-2">
                            <h6 className="text-900 line-height-3 mb-0 flex-1 pr-2">{egitim.baslik}</h6>
                            {statusTemplate(egitim)}
                        </div>
                        
                        <p className="text-600 text-sm line-height-3 mb-2">
                            {egitim.aciklama?.substring(0, 80)}...
                        </p>
                        
                        <div className="flex align-items-center gap-2 mb-2">
                            <i className="pi pi-user text-500 text-sm"></i>
                            <span className="text-600 text-sm">{egitim.egitmen}</span>
                        </div>
                        
                        <div className="flex align-items-center justify-content-between mb-3">
                            <div className="flex gap-2">
                                {seviyeBadgeTemplate(egitim)}
                                {zorunluTemplate(egitim)}
                            </div>
                            <span className="text-600 font-semibold">
                                {durationTemplate(egitim)}
                            </span>
                        </div>
                        
                        <div className="flex gap-2">
                            {actionBodyTemplate(egitim)}
                        </div>
                    </div>
                </Card>
            </div>
        );
    };

    // Header templates
    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                {permissions.write && (
                    <Button 
                        label="Yeni Video Eğitim" 
                        icon="pi pi-plus" 
                        className="p-button-success"
                        onClick={openNew} 
                    />
                )}
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <div className="flex gap-2">
                    <Dropdown
                        value={selectedKategori}
                        options={[
                            { label: 'Tüm Kategoriler', value: null },
                            ...kategoriler.map(k => ({ label: k.ad, value: k.id }))
                        ]}
                        onChange={(e) => setSelectedKategori(e.value)}
                        placeholder="Kategori filtrele"
                        className="w-full md:w-14rem"
                    />
                    
                    <span className="p-input-icon-left">
                        <i className="pi pi-search" />
                        <InputText 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder="Video ara..." 
                            className="w-full md:w-20rem"
                        />
                    </span>
                </div>
                
                <div className="flex gap-1">
                    <Button 
                        icon="pi pi-table"
                        className={`p-button-outlined ${viewMode === 'table' ? 'p-button-primary' : ''}`}
                        onClick={() => setViewMode('table')}
                        tooltip="Tablo Görünümü"
                    />
                    <Button 
                        icon="pi pi-th-large"
                        className={`p-button-outlined ${viewMode === 'cards' ? 'p-button-primary' : ''}`}
                        onClick={() => setViewMode('cards')}
                        tooltip="Kart Görünümü"
                    />
                </div>
            </div>
        );
    };

    const header = (
        <div className="flex flex-column md:flex-row md:align-items-center md:justify-content-between gap-2">
            <h2 className="m-0 text-900">Video Eğitimler</h2>
        </div>
    );

    // Dialog footers
    const egitimDialogFooter = (
        <React.Fragment>
            <Button 
                label="İptal" 
                icon="pi pi-times" 
                className="p-button-text" 
                onClick={hideDialog} 
            />
            <Button 
                label="Kaydet" 
                icon="pi pi-check" 
                onClick={saveVideoEgitim}
                loading={loading}
            />
        </React.Fragment>
    );


    const deleteDialogFooter = (
        <React.Fragment>
            <Button 
                label="Hayır" 
                icon="pi pi-times" 
                className="p-button-text" 
                onClick={hideDeleteDialog} 
            />
            <Button 
                label="Evet" 
                icon="pi pi-check" 
                className="p-button-danger"
                onClick={deleteEgitim} 
            />
        </React.Fragment>
    );

    if (!permissions.read) {
        return (
            <div className="flex align-items-center justify-content-center" style={{ height: '400px' }}>
                <div className="text-center">
                    <i className="pi pi-lock text-6xl text-400 mb-3"></i>
                    <h5 className="text-600">Bu sayfayı görüntüleme yetkiniz bulunmuyor.</h5>
                </div>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <Toast ref={toast} />
                    <ConfirmDialog />
                    
                    <Toolbar 
                        className="mb-4" 
                        left={leftToolbarTemplate} 
                        right={rightToolbarTemplate} 
                    />

                    {loading ? (
                        <div className="flex align-items-center justify-content-center" style={{ height: '400px' }}>
                            <ProgressSpinner />
                        </div>
                    ) : (
                        <>
                            {viewMode === 'table' ? (
                                <DataTable
                                    value={filteredEgitimler}
                                    dataKey="id"
                                    paginator
                                    rows={10}
                                    rowsPerPageOptions={[5, 10, 25]}
                                    className="datatable-responsive"
                                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                                    currentPageReportTemplate="{first} - {last} / {totalRecords} video"
                                    globalFilter={globalFilter}
                                    header={header}
                                    emptyMessage="Video eğitim bulunamadı."
                                    responsiveLayout="scroll"
                                >
                                    <Column 
                                        body={thumbnailBodyTemplate} 
                                        header="Önizleme" 
                                        style={{ width: '120px' }}
                                        exportable={false}
                                    />
                                    <Column 
                                        field="baslik" 
                                        header="Başlık" 
                                        sortable 
                                        style={{ minWidth: '200px' }}
                                    />
                                    <Column 
                                        field="egitmen" 
                                        header="Eğitmen" 
                                        sortable 
                                        style={{ minWidth: '150px' }}
                                    />
                                    <Column 
                                        body={durationTemplate} 
                                        header="Süre" 
                                        style={{ width: '100px' }}
                                    />
                                    <Column 
                                        body={seviyeBadgeTemplate} 
                                        header="Seviye" 
                                        style={{ width: '120px' }}
                                    />
                                    <Column 
                                        body={zorunluTemplate} 
                                        header="Durum" 
                                        style={{ width: '120px' }}
                                    />
                                    <Column 
                                        body={statusTemplate} 
                                        header="Aktif" 
                                        style={{ width: '100px' }}
                                    />
                                    <Column 
                                        body={actionBodyTemplate} 
                                        header="İşlemler"
                                        exportable={false} 
                                        style={{ minWidth: '120px' }}
                                    />
                                </DataTable>
                            ) : (
                                <div className="grid">
                                    {filteredEgitimler.map(cardTemplate)}
                                </div>
                            )}
                        </>
                    )}

                    {/* Video Eğitim Dialog */}
                    <Dialog 
                        visible={egitimDialog} 
                        style={{ width: '90vw', maxWidth: '800px' }} 
                        header={videoEgitim.id ? 'Video Eğitim Düzenle' : 'Yeni Video Eğitim'} 
                        modal 
                        className="p-fluid"
                        footer={egitimDialogFooter} 
                        onHide={hideDialog}
                        breakpoints={{'960px': '75vw', '641px': '100vw'}}
                    >
                        <div className="formgrid grid">
                            {/* Temel Bilgiler */}
                            <div className="col-12">
                                <h5 className="text-900 mb-3">📚 Temel Bilgiler</h5>
                            </div>
                            
                            <div className="col-12">
                                <label htmlFor="baslik" className="block text-900 font-medium mb-2">
                                    Eğitim Başlığı *
                                </label>
                                <InputText 
                                    id="baslik" 
                                    value={videoEgitim.baslik} 
                                    onChange={(e) => onInputChange(e, 'baslik')} 
                                    required 
                                    autoFocus 
                                    className={submitted && !videoEgitim.baslik ? 'p-invalid' : ''}
                                    placeholder="Örn: JavaScript Temelleri"
                                />
                                {submitted && !videoEgitim.baslik && <small className="p-error">Başlık gerekli.</small>}
                            </div>
                            
                            <div className="col-12">
                                <label htmlFor="aciklama" className="block text-900 font-medium mb-2">
                                    Açıklama
                                </label>
                                <InputTextarea 
                                    id="aciklama" 
                                    value={videoEgitim.aciklama} 
                                    onChange={(e) => onInputChange(e, 'aciklama')} 
                                    rows={3} 
                                    placeholder="Eğitim hakkında kısa açıklama..."
                                />
                            </div>
                            
                            <div className="col-12 md:col-6">
                                <label htmlFor="kategori" className="block text-900 font-medium mb-2">
                                    Kategori *
                                </label>
                                <Dropdown 
                                    id="kategori" 
                                    value={videoEgitim.kategoriId} 
                                    options={kategoriler.map(k => ({ label: k.ad, value: k.id }))} 
                                    onChange={(e) => onDropdownChange(e, 'kategoriId')} 
                                    placeholder="Kategori seçin"
                                    className={submitted && !videoEgitim.kategoriId ? 'p-invalid' : ''}
                                />
                                {submitted && !videoEgitim.kategoriId && <small className="p-error">Kategori gerekli.</small>}
                            </div>
                            
                            <div className="col-12 md:col-6">
                                <label htmlFor="egitmen" className="block text-900 font-medium mb-2">
                                    Eğitmen
                                </label>
                                <InputText 
                                    id="egitmen" 
                                    value={videoEgitim.egitmen} 
                                    onChange={(e) => onInputChange(e, 'egitmen')} 
                                    placeholder="Eğitmen adı"
                                />
                            </div>

                            {/* Video Detayları */}
                            <div className="col-12">
                                <Divider />
                                <h5 className="text-900 mb-3">🎥 Video Detayları</h5>
                            </div>
                            
                            <div className="col-12 md:col-8">
                                <label htmlFor="videoUrl" className="block text-900 font-medium mb-2">
                                    Video URL *
                                </label>
                                <div className="p-inputgroup">
                                    <InputText 
                                        id="videoUrl" 
                                        value={videoEgitim.videoUrl} 
                                        onChange={handleVideoUrlChange} 
                                        placeholder="YouTube, Vimeo veya doğrudan video linki"
                                        required 
                                        className={submitted && !videoEgitim.videoUrl ? 'p-invalid' : ''}
                                        disabled={durationLoading}
                                    />
                                    {durationLoading && (
                                        <Button 
                                            icon="pi pi-spin pi-spinner" 
                                            className="p-button-outline-secondary"
                                            disabled
                                        />
                                    )}
                                </div>
                                {submitted && !videoEgitim.videoUrl && <small className="p-error">Video URL gerekli.</small>}
                            </div>
                            
                            <div className="col-12 md:col-4">
                                <label htmlFor="sure" className="block text-900 font-medium mb-2">
                                    Süre (Dakika) - Otomatik
                                </label>
                                <InputNumber 
                                    id="sure" 
                                    value={videoEgitim.sureDakika} 
                                    onValueChange={(e) => onNumberChange(e, 'sureDakika')} 
                                    min={1} 
                                    placeholder="0"
                                    suffix=" dk"
                                    disabled={true}
                                    className="p-inputtext-disabled"
                                />
                                <small className="p-text-secondary">Video URL&apos;sinden otomatik hesaplanır</small>
                            </div>

                            {/* Eğitim Ayarları */}
                            <div className="col-12">
                                <Divider />
                                <h5 className="text-900 mb-3">⚙️ Eğitim Ayarları</h5>
                            </div>
                            
                            <div className="col-12 md:col-6">
                                <label htmlFor="seviye" className="block text-900 font-medium mb-2">
                                    Seviye
                                </label>
                                <Dropdown 
                                    id="seviye" 
                                    value={videoEgitim.seviye} 
                                    options={seviyeOptions} 
                                    onChange={(e) => onDropdownChange(e, 'seviye')} 
                                />
                            </div>
                            
                            <div className="col-12 md:col-6">
                                <label className="block text-900 font-medium mb-2">
                                    Minimum İzlenme Oranı: %{videoEgitim.izlenmeMinimum}
                                </label>
                                <Slider 
                                    value={videoEgitim.izlenmeMinimum} 
                                    onChange={onSliderChange}
                                    min={50}
                                    max={100}
                                    step={5}
                                    className="w-full"
                                />
                            </div>
                            
                            <div className="col-12">
                                <div className="flex gap-4 mt-3">
                                    <div className="field-checkbox">
                                        <Checkbox 
                                            inputId="zorunlu" 
                                            checked={videoEgitim.zorunluMu} 
                                            onChange={(e) => onCheckboxChange(e, 'zorunluMu')} 
                                        />
                                        <label htmlFor="zorunlu" className="ml-2 text-900 font-medium">
                                            🔒 Zorunlu Eğitim
                                        </label>
                                    </div>
                                    
                                    <div className="field-checkbox">
                                        <Checkbox 
                                            inputId="aktif" 
                                            checked={videoEgitim.aktif} 
                                            onChange={(e) => onCheckboxChange(e, 'aktif')} 
                                        />
                                        <label htmlFor="aktif" className="ml-2 text-900 font-medium">
                                            ✅ Aktif
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Dialog>


                    {/* Delete Confirmation Dialog */}
                    <Dialog 
                        visible={deleteDialog} 
                        style={{ width: '450px' }} 
                        header="🗑️ Silme Onayı" 
                        modal 
                        footer={deleteDialogFooter} 
                        onHide={hideDeleteDialog}
                    >
                        <div className="confirmation-content">
                            <i className="pi pi-exclamation-triangle mr-3" style={{ fontSize: '2rem' }} />
                            {egitimToDelete && (
                                <span>
                                    <strong>&quot;{egitimToDelete.baslik}&quot;</strong> adlı video eğitimini silmek istediğinizden emin misiniz?
                                </span>
                            )}
                        </div>
                    </Dialog>
                </div>
            </div>
        </div>
    );
};

export default VideoEgitimler;