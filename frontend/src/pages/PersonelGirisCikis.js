import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Card } from 'primereact/card';
import { Chart } from 'primereact/chart';
import { Badge } from 'primereact/badge';
import { Tag } from 'primereact/tag';
import { ProgressSpinner } from 'primereact/progressspinner';
import personelGirisCikisService from '../services/personelGirisCikisService';
import personelService from '../services/personelService';

const PersonelGirisCikis = () => {
    const [girisCikislar, setGirisCikislar] = useState([]);
    const [personelListesi, setPersonelListesi] = useState([]);
    const [selectedGirisCikis, setSelectedGirisCikis] = useState(null);
    const [girisCikisDialog, setGirisCikisDialog] = useState(false);
    const [deleteGirisCikisDialog, setDeleteGirisCikisDialog] = useState(false);
    const [loading, setLoading] = useState(true);
    const [dashboardLoading, setDashboardLoading] = useState(true);
    const [dashboardData, setDashboardData] = useState(null);
    const [globalFilter, setGlobalFilter] = useState('');
    const [chartData, setChartData] = useState({});
    const [chartOptions, setChartOptions] = useState({});
    
    const toast = useRef(null);
    const dt = useRef(null);

    const girisTipleri = [
        { label: 'Normal', value: 'Normal' },
        { label: 'Fazla Mesai', value: 'Fazla Mesai' },
        { label: 'Hafta Sonu', value: 'Hafta Sonu' }
    ];

    const emptyGirisCikis = {
        id: 0,
        personelId: null,
        girisTarihi: null,
        cikisTarihi: null,
        girisTipi: 'Normal',
        aciklama: ''
    };

    useEffect(() => {
        loadData();
        loadPersonelListesi();
        loadDashboardData();
        initChart();
    }, []);

    const initChart = () => {
        const documentStyle = getComputedStyle(document.documentElement);
        const surfaceBorder = documentStyle.getPropertyValue('--surface-border');
        
        const options = {
            maintainAspectRatio: false,
            aspectRatio: 0.6,
            plugins: {
                legend: {
                    labels: {
                        color: documentStyle.getPropertyValue('--text-color')
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        color: documentStyle.getPropertyValue('--text-color-secondary')
                    },
                    grid: {
                        color: surfaceBorder
                    }
                },
                y: {
                    ticks: {
                        color: documentStyle.getPropertyValue('--text-color-secondary')
                    },
                    grid: {
                        color: surfaceBorder
                    }
                }
            }
        };

        setChartOptions(options);
    };

    const loadData = async () => {
        try {
            setLoading(true);
            const response = await personelGirisCikisService.getAll();
            // console.log('PersonelGirisCikis Data Response:', response);
            if (response.success) {
                setGirisCikislar(response.data);
            // console.log('PersonelGirisCikis Data Loaded:', response.data);
            } else {
                showError('Veriler yüklenirken hata oluştu: ' + response.message);
            }
        } catch (error) {
            // console.error('PersonelGirisCikis Load Error:', error);
            showError('Veriler yüklenirken hata oluştu: ' + error.message);
        } finally {
            setLoading(false);
        }
    };

    const loadPersonelListesi = async () => {
        try {
            const response = await personelService.getAktif();
            if (response.success) {
                const personelOptions = response.data.map(p => ({
                    label: `${p.ad} ${p.soyad} (${p.pozisyonAd || ''})`,
                    value: p.id
                }));
                setPersonelListesi(personelOptions);
            }
        } catch (error) {
            // console.error('Personel listesi yüklenirken hata:', error);
        }
    };

    const loadDashboardData = async () => {
        try {
            setDashboardLoading(true);
            const response = await personelGirisCikisService.getDashboardData();
            // console.log('Dashboard Data Response:', response);
            if (response.success) {
                setDashboardData(response.data);
            // console.log('Dashboard Data Loaded:', response.data);
                
                // Chart data hazırla
                const gunlukData = response.data.gunlukDagilim || [];
                const chartData = {
                    labels: gunlukData.map(d => d.tarih),
                    datasets: [
                        {
                            label: 'Giriş Sayısı',
                            data: gunlukData.map(d => d.giris),
                            backgroundColor: 'rgba(54, 162, 235, 0.2)',
                            borderColor: 'rgb(54, 162, 235)',
                            borderWidth: 2
                        },
                        {
                            label: 'Ortalama Çalışma (Saat)',
                            data: gunlukData.map(d => Math.round(d.ortalamaCalisma / 60 * 10) / 10),
                            backgroundColor: 'rgba(75, 192, 192, 0.2)',
                            borderColor: 'rgb(75, 192, 192)',
                            borderWidth: 2
                        }
                    ]
                };
                setChartData(chartData);
            }
        } catch (error) {
            // console.error('Dashboard verileri yüklenirken hata:', error);
        } finally {
            setDashboardLoading(false);
        }
    };

    const showSuccess = (message) => {
        toast.current?.show({ severity: 'success', summary: 'Başarılı', detail: message, life: 3000 });
    };

    const showError = (message) => {
        toast.current?.show({ severity: 'error', summary: 'Hata', detail: message, life: 3000 });
    };

    const openNew = () => {
        setSelectedGirisCikis({ ...emptyGirisCikis });
        setGirisCikisDialog(true);
    };

    const editGirisCikis = (girisCikis) => {
        setSelectedGirisCikis({ 
            ...girisCikis,
            girisTarihi: new Date(girisCikis.girisTarihi),
            cikisTarihi: girisCikis.cikisTarihi ? new Date(girisCikis.cikisTarihi) : null
        });
        setGirisCikisDialog(true);
    };

    const confirmDeleteGirisCikis = (girisCikis) => {
        setSelectedGirisCikis(girisCikis);
        setDeleteGirisCikisDialog(true);
    };

    const saveGirisCikis = async () => {
        try {
            if (!selectedGirisCikis) return;
            
            const data = {
                ...selectedGirisCikis,
                girisTarihi: selectedGirisCikis.girisTarihi,
                cikisTarihi: selectedGirisCikis.cikisTarihi
            };

            let response;
            if (selectedGirisCikis.Id === 0) {
                response = await personelGirisCikisService.create(data);
            } else {
                response = await personelGirisCikisService.update(selectedGirisCikis.Id, data);
            }

            if (response.success) {
                showSuccess(selectedGirisCikis.Id === 0 ? 'Giriş-çıkış kaydı eklendi' : 'Giriş-çıkış kaydı güncellendi');
                setGirisCikisDialog(false);
                loadData();
                loadDashboardData();
            } else {
                showError(response.message);
            }
        } catch (error) {
            showError('Kayıt sırasında hata oluştu: ' + error.message);
        }
    };

    const deleteGirisCikis = async () => {
        try {
            if (!selectedGirisCikis) return;
            
            const response = await personelGirisCikisService.delete(selectedGirisCikis.Id);
            if (response.success) {
                showSuccess('Giriş-çıkış kaydı silindi');
                setDeleteGirisCikisDialog(false);
                loadData();
                loadDashboardData();
            } else {
                showError(response.message);
            }
        } catch (error) {
            showError('Silme işlemi sırasında hata oluştu: ' + error.message);
        }
    };

    const onInputChange = (e, name) => {
        const val = (e.target && e.target.value) || '';
        let _girisCikis = { ...selectedGirisCikis };
        _girisCikis[`${name}`] = val;
        setSelectedGirisCikis(_girisCikis);
    };

    const onDropdownChange = (e, name) => {
        const val = e.value;
        let _girisCikis = { ...selectedGirisCikis };
        _girisCikis[`${name}`] = val;
        setSelectedGirisCikis(_girisCikis);
    };

    const onDateChange = (e, name) => {
        const val = e.value;
        let _girisCikis = { ...selectedGirisCikis };
        _girisCikis[`${name}`] = val;
        setSelectedGirisCikis(_girisCikis);
    };

    const formatDate = (date) => {
        if (!date) return '-';
        return new Date(date).toLocaleString('tr-TR');
    };

    const formatDuration = (dakika) => {
        if (!dakika) return '-';
        const saat = Math.floor(dakika / 60);
        const kalanDakika = dakika % 60;
        return `${saat}s ${kalanDakika}dk`;
    };

    const girisTipiBodyTemplate = (rowData) => {
        let severity = 'info';
        switch (rowData.girisTipi) {
            case 'Normal':
                severity = 'info';
                break;
            case 'Fazla Mesai':
                severity = 'warning';
                break;
            case 'Hafta Sonu':
                severity = 'success';
                break;
        }
        return <Tag value={rowData.girisTipi} severity={severity} />;
    };

    const gecKalmaBodyTemplate = (rowData) => {
        if (rowData.gecKalmaDakika > 0) {
            return <Badge value={`${rowData.gecKalmaDakika}dk`} severity="danger" />;
        }
        return '-';
    };

    const erkenCikmaBodyTemplate = (rowData) => {
        if (rowData.erkenCikmaDakika > 0) {
            return <Badge value={`${rowData.erkenCikmaDakika}dk`} severity="warning" />;
        }
        return '-';
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                <Button icon="pi pi-pencil" className="p-button-rounded p-button-success mr-2" onClick={() => editGirisCikis(rowData)} />
                <Button icon="pi pi-trash" className="p-button-rounded p-button-warning" onClick={() => confirmDeleteGirisCikis(rowData)} />
            </React.Fragment>
        );
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                <Button label="Yeni" icon="pi pi-plus" className="p-button-success mr-2" onClick={openNew} />
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <InputText 
                        type="search" 
                        value={globalFilter} 
                        onChange={(e) => setGlobalFilter(e.target.value)} 
                        placeholder="Ara..." 
                    />
                </span>
            </React.Fragment>
        );
    };

    const hideDialog = () => {
        setGirisCikisDialog(false);
    };

    const hideDeleteDialog = () => {
        setDeleteGirisCikisDialog(false);
    };

    const girisCikisDialogFooter = (
        <React.Fragment>
            <Button label="İptal" icon="pi pi-times" className="p-button-text" onClick={hideDialog} />
            <Button label="Kaydet" icon="pi pi-check" className="p-button-text" onClick={saveGirisCikis} />
        </React.Fragment>
    );

    const deleteGirisCikisDialogFooter = (
        <React.Fragment>
            <Button label="Hayır" icon="pi pi-times" className="p-button-text" onClick={hideDeleteDialog} />
            <Button label="Evet" icon="pi pi-check" className="p-button-text" onClick={deleteGirisCikis} />
        </React.Fragment>
    );

    if (loading) {
        return (
            <div className="flex justify-content-center align-items-center" style={{ height: '200px' }}>
                <ProgressSpinner />
            </div>
        );
    }

    return (
        <div className="card">
            <Toast ref={toast} />
            
            {/* Dashboard Cards */}
            {dashboardData && (
                <div className="grid mb-4">
                    <div className="col-12 md:col-3">
                        <Card className="text-center">
                            <div className="text-900 font-medium mb-2">Bugün Toplam Personel</div>
                            <div className="text-4xl font-bold text-blue-500">
                                {dashboardData.bugunIstatistikleri?.toplamPersonel || 0}
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-3">
                        <Card className="text-center">
                            <div className="text-900 font-medium mb-2">Geç Kalanlar</div>
                            <div className="text-4xl font-bold text-red-500">
                                {dashboardData.bugunIstatistikleri?.gecKalanlar || 0}
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-3">
                        <Card className="text-center">
                            <div className="text-900 font-medium mb-2">Erken Çıkanlar</div>
                            <div className="text-4xl font-bold text-orange-500">
                                {dashboardData.bugunIstatistikleri?.erkenCikanlar || 0}
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-3">
                        <Card className="text-center">
                            <div className="text-900 font-medium mb-2">Fazla Mesai</div>
                            <div className="text-4xl font-bold text-green-500">
                                {dashboardData.bugunIstatistikleri?.fazlaMesai || 0}
                            </div>
                        </Card>
                    </div>
                </div>
            )}

            {/* Chart */}
            {chartData.labels && (
                <div className="grid mb-4">
                    <div className="col-12">
                        <Card title="Son 7 Günün Giriş-Çıkış Dağılımı">
                            <Chart type="line" data={chartData} options={chartOptions} style={{ height: '300px' }} />
                        </Card>
                    </div>
                </div>
            )}

            {/* Data Table */}
            <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate}></Toolbar>

            <DataTable 
                ref={dt}
                value={girisCikislar} 
                paginator 
                rows={10} 
                rowsPerPageOptions={[5, 10, 25]}
                globalFilter={globalFilter}
                emptyMessage="Kayıt bulunamadı."
                className="datatable-responsive"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                currentPageReportTemplate="{first} - {last} / {totalRecords} kayıt gösteriliyor"
                responsiveLayout="scroll"
            >
                <Column field="personelAdi" header="Personel" sortable />
                <Column field="departman" header="Departman" sortable />
                <Column field="pozisyon" header="Pozisyon" sortable />
                <Column 
                    field="GirisTarihi" 
                    header="Giriş Tarihi" 
                    body={(rowData) => formatDate(rowData.girisTarihi)} 
                    sortable 
                />
                <Column 
                    field="CikisTarihi" 
                    header="Çıkış Tarihi" 
                    body={(rowData) => formatDate(rowData.cikisTarihi)} 
                    sortable 
                />
                <Column field="girisTipi" header="Giriş Tipi" body={girisTipiBodyTemplate} sortable />
                <Column 
                    field="CalismaSuresiDakika" 
                    header="Çalışma Süresi" 
                    body={(rowData) => formatDuration(rowData.CalismaSuresiDakika)} 
                    sortable 
                />
                <Column field="gecKalmaDakika" header="Geç Kalma" body={gecKalmaBodyTemplate} sortable />
                <Column field="erkenCikmaDakika" header="Erken Çıkma" body={erkenCikmaBodyTemplate} sortable />
                <Column field="aciklama" header="Açıklama" sortable />
                <Column body={actionBodyTemplate} header="İşlemler" />
            </DataTable>

            {/* Add/Edit Dialog */}
            <Dialog 
                visible={girisCikisDialog} 
                style={{ width: '450px' }} 
                header="Giriş-Çıkış Detayı" 
                modal 
                className="p-fluid" 
                footer={girisCikisDialogFooter} 
                onHide={hideDialog}
            >
                <div className="field">
                    <label htmlFor="personel">Personel *</label>
                    <Dropdown 
                        id="personel"
                        value={selectedGirisCikis?.personelId} 
                        options={personelListesi} 
                        onChange={(e) => onDropdownChange(e, 'personelId')} 
                        placeholder="Personel seçin"
                        filter
                        showClear
                    />
                </div>

                <div className="field">
                    <label htmlFor="girisTarihi">Giriş Tarihi *</label>
                    <Calendar
                        id="girisTarihi"
                        value={selectedGirisCikis?.girisTarihi}
                        onChange={(e) => onDateChange(e, 'girisTarihi')}
                        showTime
                        showSeconds={false}
                        dateFormat="dd/mm/yy"
                        timeFormat="24"
                        placeholder="dd/mm/yyyy ss:dd"
                        locale="tr"
                    />
                </div>

                <div className="field">
                    <label htmlFor="cikisTarihi">Çıkış Tarihi</label>
                    <Calendar
                        id="cikisTarihi"
                        value={selectedGirisCikis?.cikisTarihi}
                        onChange={(e) => onDateChange(e, 'cikisTarihi')}
                        showTime
                        showSeconds={false}
                        dateFormat="dd/mm/yy"
                        timeFormat="24"
                        placeholder="dd/mm/yyyy ss:dd"
                        locale="tr"
                    />
                </div>

                <div className="field">
                    <label htmlFor="girisTipi">Giriş Tipi</label>
                    <Dropdown 
                        id="girisTipi"
                        value={selectedGirisCikis?.girisTipi} 
                        options={girisTipleri} 
                        onChange={(e) => onDropdownChange(e, 'girisTipi')} 
                        placeholder="Giriş tipi seçin"
                    />
                </div>

                <div className="field">
                    <label htmlFor="aciklama">Açıklama</label>
                    <InputTextarea 
                        id="aciklama"
                        value={selectedGirisCikis?.aciklama || ''} 
                        onChange={(e) => onInputChange(e, 'aciklama')} 
                        rows={3} 
                        cols={20} 
                    />
                </div>
            </Dialog>

            {/* Delete Dialog */}
            <Dialog 
                visible={deleteGirisCikisDialog} 
                style={{ width: '450px' }} 
                header="Onayla" 
                modal 
                footer={deleteGirisCikisDialogFooter} 
                onHide={hideDeleteDialog}
            >
                <div className="confirmation-content">
                    <i className="pi pi-exclamation-triangle mr-3" style={{ fontSize: '2rem'}} />
                    {selectedGirisCikis && <span><b>{selectedGirisCikis.PersonelAdi}</b> personelinin giriş-çıkış kaydını silmek istediğinizden emin misiniz?</span>}
                </div>
            </Dialog>
        </div>
    );
};

export default PersonelGirisCikis;