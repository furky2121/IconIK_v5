import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Tag } from 'primereact/tag';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import { TabView, TabPanel } from 'primereact/tabview';
import { ProgressBar } from 'primereact/progressbar';
import bordroService from '../services/bordroService';
import personelService from '../services/personelService';
import yetkiService from '../services/yetkiService';

const BordroHazirlama = () => {
    const emptyBordro = {
        personelId: null,
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        brutUcret: 0,
        netUcret: 0
    };

    const [bordrolar, setBordrolar] = useState([]);
    const [personeller, setPersoneller] = useState([]);
    const [hesaplaDialog, setHesaplaDialog] = useState(false);
    const [brutNetDialog, setBrutNetDialog] = useState(false);
    const [detailDialog, setDetailDialog] = useState(false);
    const [topluHesaplaDialog, setTopluHesaplaDialog] = useState(false);
    const [selectedBordro, setSelectedBordro] = useState(null);
    const [bordroForm, setBordroForm] = useState(emptyBordro);
    const [hesaplamaTuru, setHesaplamaTuru] = useState('brut');
    const [hesaplamaGirdi, setHesaplamaGirdi] = useState(0);
    const [hesaplamaSonuc, setHesaplamaSonuc] = useState(null);
    const [filters, setFilters] = useState({
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        personelId: null
    });
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [processing, setProcessing] = useState(false);
    const [topluProgress, setTopluProgress] = useState(0);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
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
                read: yetkiService.hasScreenPermission('bordro-hazirlama', 'read'),
                write: yetkiService.hasScreenPermission('bordro-hazirlama', 'write'),
                delete: yetkiService.hasScreenPermission('bordro-hazirlama', 'delete'),
                update: yetkiService.hasScreenPermission('bordro-hazirlama', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                write: false,
                delete: false,
                update: false
            });
        }
    };

    const loadBordrolar = async () => {
        setLoading(true);
        try {
            const response = await bordroService.getAll(filters);
            if (response.success) {
                setBordrolar(response.data);
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

    const openHesaplaDialog = () => {
        setBordroForm(emptyBordro);
        setHesaplaDialog(true);
    };

    const hideHesaplaDialog = () => {
        setHesaplaDialog(false);
        setBordroForm(emptyBordro);
    };

    const openBrutNetDialog = () => {
        setHesaplamaGirdi(0);
        setHesaplamaSonuc(null);
        setBrutNetDialog(true);
    };

    const hideBrutNetDialog = () => {
        setBrutNetDialog(false);
        setHesaplamaGirdi(0);
        setHesaplamaSonuc(null);
    };

    const openTopluHesaplaDialog = () => {
        setTopluHesaplaDialog(true);
        setTopluProgress(0);
    };

    const hideTopluHesaplaDialog = () => {
        setTopluHesaplaDialog(false);
        setTopluProgress(0);
    };

    const hesaplaBordro = async () => {
        if (!bordroForm.personelId || !bordroForm.brutUcret || processing) return;

        setProcessing(true);
        try {
            const response = await bordroService.hesapla(bordroForm);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Bordro başarıyla hesaplandı',
                    life: 3000
                });
                loadBordrolar();
                hideHesaplaDialog();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        } finally {
            setProcessing(false);
        }
    };

    const hesaplaBrutNet = async () => {
        if (!hesaplamaGirdi || processing) return;

        setProcessing(true);
        try {
            const data = {
                [hesaplamaTuru === 'brut' ? 'brutUcret' : 'netUcret']: hesaplamaGirdi,
                yil: filters.yil,
                ay: filters.ay
            };

            const response = await bordroService.hesaplaBrutNet(data);
            if (response.success) {
                setHesaplamaSonuc(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        } finally {
            setProcessing(false);
        }
    };

    const topluHesapla = async () => {
        if (processing) return;

        confirmDialog({
            message: `${filters.yil}/${ayOptions.find(a => a.value === filters.ay)?.label} dönemi için TÜM personellerin bordrosunu hesaplamak istediğinizden emin misiniz?`,
            header: 'Toplu Hesaplama Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: async () => {
                setProcessing(true);
                setTopluProgress(10);
                try {
                    const response = await bordroService.hesaplaToplu({
                        yil: filters.yil,
                        ay: filters.ay
                    });

                    setTopluProgress(90);

                    if (response.success) {
                        setTopluProgress(100);
                        toast.current.show({
                            severity: 'success',
                            summary: 'Başarılı',
                            detail: `${response.data?.hesaplananSayi || 0} adet bordro başarıyla hesaplandı`,
                            life: 4000
                        });
                        setTimeout(() => {
                            loadBordrolar();
                            hideTopluHesaplaDialog();
                        }, 1000);
                    }
                } catch (error) {
                    toast.current.show({
                        severity: 'error',
                        summary: 'Hata',
                        detail: error.message,
                        life: 3000
                    });
                    setTopluProgress(0);
                } finally {
                    setProcessing(false);
                }
            }
        });
    };

    const showBordroDetail = (bordro) => {
        setSelectedBordro(bordro);
        setDetailDialog(true);
    };

    const hideDetailDialog = () => {
        setDetailDialog(false);
        setSelectedBordro(null);
    };

    const confirmDeleteBordro = (bordro) => {
        confirmDialog({
            message: `${bordro.personelAd} personeline ait ${bordro.yil}/${bordro.ay} bordrosunu silmek istediğinizden emin misiniz?`,
            header: 'Silme Onayı',
            icon: 'pi pi-exclamation-triangle',
            accept: () => deleteBordro(bordro.id)
        });
    };

    const deleteBordro = async (id) => {
        try {
            const response = await bordroService.delete(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadBordrolar();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message,
                life: 3000
            });
        }
    };

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const onFilterChange = (e, name) => {
        const val = (e.target && e.target.value) !== undefined ? e.target.value : e.value;
        let _filters = { ...filters };
        _filters[`${name}`] = val;
        setFilters(_filters);
    };

    const onBordroFormChange = (e, name) => {
        const val = (e.target && e.target.value) !== undefined ? e.target.value : e.value;
        let _bordro = { ...bordroForm };
        _bordro[`${name}`] = val;
        setBordroForm(_bordro);
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.write && (
                    <>
                        <Button
                            label="Bordro Hesapla"
                            icon="pi pi-calculator"
                            className="p-button-success p-mr-2"
                            onClick={openHesaplaDialog}
                        />
                        <Button
                            label="Toplu Hesapla"
                            icon="pi pi-users"
                            className="p-button-primary p-mr-2"
                            onClick={openTopluHesaplaDialog}
                        />
                        <Button
                            label="Brüt ⇄ Net Hesaplama"
                            icon="pi pi-sync"
                            className="p-button-info"
                            onClick={openBrutNetDialog}
                        />
                    </>
                )}
            </React.Fragment>
        );
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
        const getSeverity = (status) => {
            switch (status) {
                case 'Onaylandi':
                case 'Onaylandı':
                    return 'success';
                case 'Reddedildi':
                    return 'danger';
                case 'Beklemede':
                    return 'warning';
                default:
                    return 'info';
            }
        };

        return <Tag value={rowData.onayDurumu || 'Beklemede'} severity={getSeverity(rowData.onayDurumu)} />;
    };

    const ayBodyTemplate = (rowData) => {
        const ay = ayOptions.find(a => a.value === rowData.ay);
        return ay ? ay.label : rowData.ay;
    };

    const currencyBodyTemplate = (rowData, field) => {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(rowData[field] || 0);
    };

    const actionBodyTemplate = (rowData) => {
        const isApproved = rowData.onayDurumu === 'Onaylandi' || rowData.onayDurumu === 'Onaylandı';

        return (
            <React.Fragment>
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-mr-2"
                    onClick={() => showBordroDetail(rowData)}
                    tooltip="Detay"
                />
                {permissions.delete && !isApproved && (
                    <Button
                        icon="pi pi-trash"
                        className="p-button-rounded p-button-warning"
                        onClick={() => confirmDeleteBordro(rowData)}
                        tooltip="Sil"
                    />
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Bordro Hazırlama</h5>
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

    const hesaplaDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideHesaplaDialog}
                disabled={processing}
            />
            <Button
                label="Hesapla"
                icon="pi pi-check"
                className="p-button-success"
                onClick={hesaplaBordro}
                disabled={processing}
            />
        </React.Fragment>
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
                    left={leftToolbarTemplate}
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
                        style={{ minWidth: '10rem' }}
                    ></Column>
                </DataTable>
            </Card>

            {/* Hesapla Dialog */}
            <Dialog
                visible={hesaplaDialog}
                style={{ width: '550px' }}
                header="Bordro Hesapla"
                modal
                className="p-fluid"
                footer={hesaplaDialogFooter}
                onHide={hideHesaplaDialog}
            >
                <div className="p-field">
                    <label htmlFor="personelId">Personel *</label>
                    <Dropdown
                        id="personelId"
                        value={bordroForm.personelId}
                        options={personeller}
                        onChange={(e) => onBordroFormChange(e, 'personelId')}
                        placeholder="Personel seçin"
                        filter
                    />
                </div>

                <div className="p-grid p-fluid">
                    <div className="p-col-6">
                        <div className="p-field">
                            <label htmlFor="yil">Yıl *</label>
                            <InputNumber
                                id="yil"
                                value={bordroForm.yil}
                                onValueChange={(e) => onBordroFormChange(e, 'yil')}
                                useGrouping={false}
                            />
                        </div>
                    </div>

                    <div className="p-col-6">
                        <div className="p-field">
                            <label htmlFor="ay">Ay *</label>
                            <Dropdown
                                id="ay"
                                value={bordroForm.ay}
                                options={ayOptions}
                                onChange={(e) => onBordroFormChange(e, 'ay')}
                                placeholder="Ay seçin"
                            />
                        </div>
                    </div>
                </div>

                <div className="p-field">
                    <label htmlFor="brutUcret">Brüt Ücret (TL) *</label>
                    <InputNumber
                        id="brutUcret"
                        value={bordroForm.brutUcret}
                        onValueChange={(e) => onBordroFormChange(e, 'brutUcret')}
                        mode="currency"
                        currency="TRY"
                        locale="tr-TR"
                    />
                </div>
            </Dialog>

            {/* Brüt-Net Dialog */}
            <Dialog
                visible={brutNetDialog}
                style={{ width: '600px' }}
                header="Brüt ⇄ Net Hesaplama"
                modal
                className="p-fluid"
                onHide={hideBrutNetDialog}
            >
                <TabView>
                    <TabPanel header="Brüt → Net">
                        <div className="p-field">
                            <label htmlFor="brutGirdi">Brüt Ücret (TL)</label>
                            <InputNumber
                                id="brutGirdi"
                                value={hesaplamaGirdi}
                                onValueChange={(e) => setHesaplamaGirdi(e.value)}
                                mode="currency"
                                currency="TRY"
                                locale="tr-TR"
                            />
                        </div>
                        <Button
                            label="Hesapla"
                            icon="pi pi-calculator"
                            onClick={() => { setHesaplamaTuru('brut'); hesaplaBrutNet(); }}
                            disabled={processing}
                        />
                    </TabPanel>
                    <TabPanel header="Net → Brüt">
                        <div className="p-field">
                            <label htmlFor="netGirdi">Net Ücret (TL)</label>
                            <InputNumber
                                id="netGirdi"
                                value={hesaplamaGirdi}
                                onValueChange={(e) => setHesaplamaGirdi(e.value)}
                                mode="currency"
                                currency="TRY"
                                locale="tr-TR"
                            />
                        </div>
                        <Button
                            label="Hesapla"
                            icon="pi pi-calculator"
                            onClick={() => { setHesaplamaTuru('net'); hesaplaBrutNet(); }}
                            disabled={processing}
                        />
                    </TabPanel>
                </TabView>

                {hesaplamaSonuc && (
                    <Panel header="Hesaplama Sonucu" className="p-mt-3">
                        <div className="p-grid">
                            <div className="p-col-6"><strong>Brüt Ücret:</strong></div>
                            <div className="p-col-6">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.brutUcret || 0)}</div>

                            <div className="p-col-6"><strong>SGK İşçi:</strong></div>
                            <div className="p-col-6">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.sgkIsci || 0)}</div>

                            <div className="p-col-6"><strong>İşsizlik İşçi:</strong></div>
                            <div className="p-col-6">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.issizlikIsci || 0)}</div>

                            <div className="p-col-6"><strong>Gelir Vergisi:</strong></div>
                            <div className="p-col-6">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.gelirVergisi || 0)}</div>

                            <div className="p-col-6"><strong>Damga Vergisi:</strong></div>
                            <div className="p-col-6">{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.damgaVergisi || 0)}</div>

                            <div className="p-col-12"><Divider /></div>

                            <div className="p-col-6"><strong>Net Ücret:</strong></div>
                            <div className="p-col-6"><strong>{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(hesaplamaSonuc.netUcret || 0)}</strong></div>
                        </div>
                    </Panel>
                )}
            </Dialog>

            {/* Toplu Hesaplama Dialog */}
            <Dialog
                visible={topluHesaplaDialog}
                style={{ width: '500px' }}
                header="Toplu Bordro Hesaplama"
                modal
                onHide={hideTopluHesaplaDialog}
                closable={!processing}
            >
                <div className="p-text-center p-p-4">
                    <h5>{filters.yil} / {ayOptions.find(a => a.value === filters.ay)?.label}</h5>
                    <p>Tüm aktif personeller için bordro hesaplanacak.</p>

                    {processing && (
                        <div className="p-mt-4">
                            <ProgressBar value={topluProgress} />
                            <p className="p-mt-2">İşlem devam ediyor...</p>
                        </div>
                    )}

                    {!processing && (
                        <div className="p-mt-4">
                            <Button
                                label="Hesaplamayı Başlat"
                                icon="pi pi-play"
                                className="p-button-success"
                                onClick={topluHesapla}
                            />
                        </div>
                    )}
                </div>
            </Dialog>

            {/* Detail Dialog */}
            <Dialog
                visible={detailDialog}
                style={{ width: '700px' }}
                header="Bordro Detayları"
                modal
                onHide={hideDetailDialog}
            >
                {selectedBordro && (
                    <div>
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

                        <Divider />

                        <div className="p-grid">
                            <div className="p-col-6">
                                <strong>Brüt Ücret:</strong>
                                <p>{currencyBodyTemplate(selectedBordro, 'brutUcret')}</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Net Ücret:</strong>
                                <p className="p-text-bold">{currencyBodyTemplate(selectedBordro, 'netUcret')}</p>
                            </div>
                        </div>

                        <Divider />

                        <div className="p-grid">
                            <div className="p-col-12">
                                <strong>Onay Durumu:</strong>
                                <div className="p-mt-2">
                                    {onayDurumuBodyTemplate(selectedBordro)}
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </Dialog>
        </div>
    );
};

export default BordroHazirlama;
