import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Tag } from 'primereact/tag';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import bordroService from '../services/bordroService';
import personelService from '../services/personelService';
import yetkiService from '../services/yetkiService';

const BordroGoruntuleme = () => {
    const [bordrolar, setBordrolar] = useState([]);
    const [personeller, setPersoneller] = useState([]);
    const [detailDialog, setDetailDialog] = useState(false);
    const [selectedBordro, setSelectedBordro] = useState(null);
    const [filters, setFilters] = useState({
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        personelId: null
    });
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false
    });
    const toast = useRef(null);
    const dt = useRef(null);

    const ayOptions = [
        { label: 'Ocak', value: 1 },
        { label: 'Şubat', value: 2 },
        { label: 'Mart', value: 3 },
        { label: 'Nisan', value: 4 },
        { label: 'Mayıs', value: 5 },
        { label: 'Haziran', value: 6 },
        { label: 'Temmuz', value: 7 },
        { label: 'Ağustos', value: 8 },
        { label: 'Eylül', value: 9 },
        { label: 'Ekim', value: 10 },
        { label: 'Kasım', value: 11 },
        { label: 'Aralık', value: 12 }
    ];

    useEffect(() => {
        loadBordrolar();
        loadPersoneller();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bordro-goruntuleme', 'read')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false
            });
        }
    };

    const loadBordrolar = async () => {
        setLoading(true);
        try {
            const response = await bordroService.getAll(filters);
            if (response.success) {
                // Sadece onaylanmış bordroları göster
                const onayliBordrolar = response.data.filter(b =>
                    b.onayDurumu === 'Onaylandi' || b.onayDurumu === 'Onaylandı'
                );
                setBordrolar(onayliBordrolar);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Bordrolar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const loadPersoneller = async () => {
        try {
            const response = await personelService.getAktifPersoneller();
            if (response.success) {
                setPersoneller(response.data.map(p => ({
                    label: `${p.ad} ${p.soyad}`,
                    value: p.id
                })));
            }
        } catch (error) {
            // console.error('Personeller yüklenirken hata:', error);
        }
    };

    const applyFilters = () => {
        loadBordrolar();
    };

    const resetFilters = () => {
        setFilters({
            yil: new Date().getFullYear(),
            ay: new Date().getMonth() + 1,
            personelId: null
        });
    };

    const onFilterChange = (e, name) => {
        const val = (e.target && e.target.value) !== undefined ? e.target.value : e.value;
        let _filters = { ...filters };
        _filters[`${name}`] = val;
        setFilters(_filters);
    };

    const showBordroDetail = async (bordro) => {
        try {
            const response = await bordroService.getById(bordro.id);
            if (response.success) {
                setSelectedBordro(response.data);
                setDetailDialog(true);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Bordro detayları yüklenirken hata oluştu',
                life: 3000
            });
        }
    };

    const hideDetailDialog = () => {
        setDetailDialog(false);
        setSelectedBordro(null);
    };

    const printBordro = () => {
        if (!selectedBordro) return;

        // Yazdırma penceresi için HTML oluştur
        const printWindow = window.open('', '', 'width=800,height=600');
        const printContent = generatePrintContent(selectedBordro);

        printWindow.document.write(printContent);
        printWindow.document.close();
        printWindow.focus();
        printWindow.print();
    };

    const generatePrintContent = (bordro) => {
        const ay = ayOptions.find(a => a.value === bordro.ay)?.label || bordro.ay;

        let odemelerHTML = '';
        if (bordro.odemeler && bordro.odemeler.length > 0) {
            odemelerHTML = bordro.odemeler.map(o => `
                <tr>
                    <td>${o.odemeAd}</td>
                    <td style="text-align: right;">${formatCurrency(o.tutar)}</td>
                </tr>
            `).join('');
        }

        let kesintilerHTML = '';
        if (bordro.kesintiler && bordro.kesintiler.length > 0) {
            kesintilerHTML = bordro.kesintiler.map(k => `
                <tr>
                    <td>${k.kesintiAd}</td>
                    <td style="text-align: right; color: #dc3545;">-${formatCurrency(k.tutar)}</td>
                </tr>
            `).join('');
        }

        return `
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <title>Bordro Fişi - ${bordro.personelAd} - ${bordro.yil}/${ay}</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        padding: 20px;
                    }
                    .header {
                        text-align: center;
                        margin-bottom: 30px;
                        border-bottom: 2px solid #333;
                        padding-bottom: 20px;
                    }
                    .header h1 {
                        margin: 0;
                        color: #333;
                    }
                    .info-section {
                        margin-bottom: 20px;
                    }
                    .info-row {
                        display: flex;
                        justify-content: space-between;
                        padding: 8px 0;
                        border-bottom: 1px solid #eee;
                    }
                    .info-label {
                        font-weight: bold;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        margin: 20px 0;
                    }
                    th {
                        background-color: #f8f9fa;
                        padding: 12px;
                        text-align: left;
                        border-bottom: 2px solid #333;
                    }
                    td {
                        padding: 10px;
                        border-bottom: 1px solid #eee;
                    }
                    .total-section {
                        margin-top: 30px;
                        padding: 20px;
                        background-color: #f8f9fa;
                        border: 2px solid #333;
                    }
                    .total-row {
                        display: flex;
                        justify-content: space-between;
                        padding: 8px 0;
                        font-size: 18px;
                    }
                    .net-total {
                        font-weight: bold;
                        font-size: 24px;
                        color: #28a745;
                        margin-top: 10px;
                        padding-top: 10px;
                        border-top: 2px solid #333;
                    }
                    .footer {
                        margin-top: 50px;
                        text-align: center;
                        color: #666;
                        font-size: 12px;
                    }
                    @media print {
                        body { margin: 0; }
                    }
                </style>
            </head>
            <body>
                <div class="header">
                    <h1>Icon</h1>
                    <h2>BORDRO FİŞİ</h2>
                </div>

                <div class="info-section">
                    <div class="info-row">
                        <span class="info-label">Personel:</span>
                        <span>${bordro.personelAd}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Dönem:</span>
                        <span>${bordro.yil} / ${ay}</span>
                    </div>
                    <div class="info-row">
                        <span class="info-label">Tarih:</span>
                        <span>${new Date().toLocaleDateString('tr-TR')}</span>
                    </div>
                </div>

                ${odemelerHTML ? `
                    <h3>Ödemeler</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Ödeme Türü</th>
                                <th style="text-align: right;">Tutar</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${odemelerHTML}
                        </tbody>
                    </table>
                ` : ''}

                ${kesintilerHTML ? `
                    <h3>Kesintiler</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Kesinti Türü</th>
                                <th style="text-align: right;">Tutar</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${kesintilerHTML}
                        </tbody>
                    </table>
                ` : ''}

                <div class="total-section">
                    <div class="total-row">
                        <span>Brüt Ücret:</span>
                        <span>${formatCurrency(bordro.brutUcret)}</span>
                    </div>
                    <div class="total-row net-total">
                        <span>NET ÜCRET:</span>
                        <span>${formatCurrency(bordro.netUcret)}</span>
                    </div>
                </div>

                <div class="footer">
                    <p>Bu belge elektronik ortamda oluşturulmuştur.</p>
                    <p>Icon İK Yönetim Sistemi</p>
                </div>
            </body>
            </html>
        `;
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(value || 0);
    };

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <Button
                    label="Dışa Aktar"
                    icon="pi pi-upload"
                    className="p-button-help"
                    onClick={exportCSV}
                />
            </React.Fragment>
        );
    };

    const onayDurumuBodyTemplate = (rowData) => {
        return <Tag value="Onaylandı" severity="success" />;
    };

    const ayBodyTemplate = (rowData) => {
        const ay = ayOptions.find(a => a.value === rowData.ay);
        return ay ? ay.label : rowData.ay;
    };

    const currencyBodyTemplate = (rowData, field) => {
        return formatCurrency(rowData[field]);
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <React.Fragment>
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-mr-2"
                    onClick={() => showBordroDetail(rowData)}
                    tooltip="Detay"
                />
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Bordro Görüntüleme</h5>
            <span className="p-input-icon-left">
                <i className="pi pi-search" />
                <InputText
                    type="search"
                    onInput={(e) => setGlobalFilter(e.target.value)}
                    placeholder="Arama yapın..."
                />
            </span>
        </div>
    );

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <Card className="p-mb-3">
                <h6>Filtreler</h6>
                <div className="p-grid p-fluid">
                    <div className="p-col-12 p-md-3">
                        <label htmlFor="filterYil">Yıl</label>
                        <InputNumber
                            id="filterYil"
                            value={filters.yil}
                            onValueChange={(e) => onFilterChange(e, 'yil')}
                            useGrouping={false}
                        />
                    </div>

                    <div className="p-col-12 p-md-3">
                        <label htmlFor="filterAy">Ay</label>
                        <Dropdown
                            id="filterAy"
                            value={filters.ay}
                            options={ayOptions}
                            onChange={(e) => onFilterChange(e, 'ay')}
                            placeholder="Ay seçin"
                        />
                    </div>

                    <div className="p-col-12 p-md-4">
                        <label htmlFor="filterPersonel">Personel</label>
                        <Dropdown
                            id="filterPersonel"
                            value={filters.personelId}
                            options={personeller}
                            onChange={(e) => onFilterChange(e, 'personelId')}
                            placeholder="Tüm Personeller"
                            filter
                            showClear
                        />
                    </div>

                    <div className="p-col-12 p-md-2">
                        <label>&nbsp;</label>
                        <div>
                            <Button
                                label="Filtrele"
                                icon="pi pi-search"
                                onClick={applyFilters}
                                className="p-mr-2"
                            />
                            <Button
                                label="Temizle"
                                icon="pi pi-times"
                                className="p-button-secondary"
                                onClick={resetFilters}
                            />
                        </div>
                    </div>
                </div>
            </Card>

            <Card>
                <Toolbar
                    className="p-mb-4"
                    right={rightToolbarTemplate}
                ></Toolbar>

                <DataTable
                    ref={dt}
                    value={bordrolar}
                    dataKey="id"
                    paginator
                    rows={10}
                    rowsPerPageOptions={[5, 10, 25]}
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} kayıt"
                    globalFilter={globalFilter}
                    header={header}
                    responsiveLayout="scroll"
                    loading={loading}
                >
                    <Column
                        field="id"
                        header="ID"
                        sortable
                        style={{ minWidth: '4rem', width: '4rem' }}
                    ></Column>
                    <Column
                        field="personelAd"
                        header="Personel"
                        sortable
                        style={{ minWidth: '14rem' }}
                    ></Column>
                    <Column
                        field="yil"
                        header="Yıl"
                        sortable
                        style={{ minWidth: '6rem' }}
                    ></Column>
                    <Column
                        field="ay"
                        header="Ay"
                        body={ayBodyTemplate}
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        field="brutUcret"
                        header="Brüt Ücret"
                        body={(rowData) => currencyBodyTemplate(rowData, 'brutUcret')}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="netUcret"
                        header="Net Ücret"
                        body={(rowData) => currencyBodyTemplate(rowData, 'netUcret')}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        field="onayDurumu"
                        header="Durum"
                        body={onayDurumuBodyTemplate}
                        sortable
                        style={{ minWidth: '10rem' }}
                    ></Column>
                    <Column
                        body={actionBodyTemplate}
                        header="İşlemler"
                        style={{ minWidth: '8rem' }}
                    ></Column>
                </DataTable>
            </Card>

            {/* Detail Dialog with Print */}
            <Dialog
                visible={detailDialog}
                style={{ width: '800px' }}
                header="Bordro Detayları"
                modal
                className="p-fluid"
                onHide={hideDetailDialog}
            >
                {selectedBordro && (
                    <div>
                        <div className="p-text-right p-mb-3">
                            <Button
                                label="Yazdır"
                                icon="pi pi-print"
                                className="p-button-primary"
                                onClick={printBordro}
                            />
                        </div>

                        <Panel header="Genel Bilgiler" className="p-mb-3">
                            <div className="p-grid">
                                <div className="p-col-6">
                                    <strong>Personel:</strong>
                                    <p>{selectedBordro.personelAd}</p>
                                </div>
                                <div className="p-col-6">
                                    <strong>Dönem:</strong>
                                    <p>{selectedBordro.yil} / {ayOptions.find(a => a.value === selectedBordro.ay)?.label}</p>
                                </div>
                            </div>
                        </Panel>

                        <Panel header="Ücret Bilgileri" className="p-mb-3">
                            <div className="p-grid">
                                <div className="p-col-6">
                                    <strong>Brüt Ücret:</strong>
                                    <p className="p-text-bold">{currencyBodyTemplate(selectedBordro, 'brutUcret')}</p>
                                </div>
                                <div className="p-col-6">
                                    <strong>Net Ücret:</strong>
                                    <p className="p-text-bold" style={{ color: '#28a745', fontSize: '1.2em' }}>
                                        {currencyBodyTemplate(selectedBordro, 'netUcret')}
                                    </p>
                                </div>
                            </div>
                        </Panel>

                        {selectedBordro.odemeler && selectedBordro.odemeler.length > 0 && (
                            <Panel header="Ödemeler" className="p-mb-3">
                                {selectedBordro.odemeler.map((odeme, index) => (
                                    <div key={index} className="p-grid">
                                        <div className="p-col-8">{odeme.odemeAd}</div>
                                        <div className="p-col-4 p-text-right">
                                            {formatCurrency(odeme.tutar)}
                                        </div>
                                    </div>
                                ))}
                            </Panel>
                        )}

                        {selectedBordro.kesintiler && selectedBordro.kesintiler.length > 0 && (
                            <Panel header="Kesintiler" className="p-mb-3">
                                {selectedBordro.kesintiler.map((kesinti, index) => (
                                    <div key={index} className="p-grid">
                                        <div className="p-col-8">{kesinti.kesintiAd}</div>
                                        <div className="p-col-4 p-text-right" style={{ color: '#dc3545' }}>
                                            -{formatCurrency(kesinti.tutar)}
                                        </div>
                                    </div>
                                ))}
                            </Panel>
                        )}

                        <Panel header="Onay Bilgileri">
                            <div className="p-grid">
                                <div className="p-col-12">
                                    <strong>Onay Durumu:</strong>
                                    <div className="p-mt-2">
                                        {onayDurumuBodyTemplate(selectedBordro)}
                                    </div>
                                </div>
                                {selectedBordro.onayNotu && (
                                    <div className="p-col-12 p-mt-2">
                                        <strong>Onay Notu:</strong>
                                        <p>{selectedBordro.onayNotu}</p>
                                    </div>
                                )}
                            </div>
                        </Panel>
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default BordroGoruntuleme;
