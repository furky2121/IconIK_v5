import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Tag } from 'primereact/tag';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import { Checkbox } from 'primereact/checkbox';
import bordroService from '../services/bordroService';
import yetkiService from '../services/yetkiService';

const BordroOnayi = () => {
    const [bordrolar, setBordrolar] = useState([]);
    const [detailDialog, setDetailDialog] = useState(false);
    const [approvalDialog, setApprovalDialog] = useState(false);
    const [selectedBordro, setSelectedBordro] = useState(null);
    const [selectedBordrolar, setSelectedBordrolar] = useState([]);
    const [onayNotu, setOnayNotu] = useState('');
    const [isApproving, setIsApproving] = useState(true);
    const [filters, setFilters] = useState({
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1,
        personelId: null
    });
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [processing, setProcessing] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false,
        approve: false
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
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bordro-onayi', 'read'),
                approve: yetkiService.hasScreenPermission('bordro-onayi', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                approve: false
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

    const confirmApproval = (bordro, isApprove) => {
        setSelectedBordro(bordro);
        setSelectedBordrolar([bordro]);
        setIsApproving(isApprove);
        setOnayNotu('');
        setApprovalDialog(true);
    };

    const confirmBulkApproval = (isApprove) => {
        if (selectedBordrolar.length === 0) {
            toast.current.show({
                severity: 'warn',
                summary: 'Uyarı',
                detail: 'Lütfen en az bir bordro seçin',
                life: 3000
            });
            return;
        }

        setIsApproving(isApprove);
        setOnayNotu('');
        setApprovalDialog(true);
    };

    const hideApprovalDialog = () => {
        setApprovalDialog(false);
        setSelectedBordro(null);
        setOnayNotu('');
    };

    const handleApproval = async () => {
        if (processing) return;

        setProcessing(true);
        try {
            const data = {
                onayNotu: onayNotu
            };

            const promises = selectedBordrolar.map(bordro => {
                if (isApproving) {
                    return bordroService.onayla(bordro.id, data);
                } else {
                    return bordroService.reddet(bordro.id, data);
                }
            });

            const results = await Promise.all(promises);
            const successCount = results.filter(r => r.success).length;

            if (successCount > 0) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: `${successCount} adet bordro ${isApproving ? 'onaylandı' : 'reddedildi'}`,
                    life: 3000
                });
                loadBordrolar();
                setSelectedBordrolar([]);
                hideApprovalDialog();
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

    const exportCSV = () => {
        dt.current.exportCSV();
    };

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                {permissions.approve && (
                    <>
                        <Button
                            label="Toplu Onayla"
                            icon="pi pi-check"
                            className="p-button-success p-mr-2"
                            onClick={() => confirmBulkApproval(true)}
                            disabled={!selectedBordrolar || selectedBordrolar.length === 0}
                        />
                        <Button
                            label="Toplu Reddet"
                            icon="pi pi-times"
                            className="p-button-danger"
                            onClick={() => confirmBulkApproval(false)}
                            disabled={!selectedBordrolar || selectedBordrolar.length === 0}
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
        const isPending = rowData.onayDurumu === 'Beklemede' || !rowData.onayDurumu;

        return (
            <React.Fragment>
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-mr-2"
                    onClick={() => showBordroDetail(rowData)}
                    tooltip="Detay"
                />
                {permissions.approve && isPending && (
                    <>
                        <Button
                            icon="pi pi-check"
                            className="p-button-rounded p-button-success p-mr-2"
                            onClick={() => confirmApproval(rowData, true)}
                            tooltip="Onayla"
                        />
                        <Button
                            icon="pi pi-times"
                            className="p-button-rounded p-button-danger"
                            onClick={() => confirmApproval(rowData, false)}
                            tooltip="Reddet"
                        />
                    </>
                )}
            </React.Fragment>
        );
    };

    const header = (
        <div className="table-header">
            <h5 className="p-m-0">Bordro Onay İşlemleri</h5>
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

    const approvalDialogFooter = (
        <React.Fragment>
            <Button
                label="İptal"
                icon="pi pi-times"
                className="p-button-text"
                onClick={hideApprovalDialog}
                disabled={processing}
            />
            <Button
                label={isApproving ? "Onayla" : "Reddet"}
                icon={isApproving ? "pi pi-check" : "pi pi-times"}
                className={isApproving ? "p-button-success" : "p-button-danger"}
                onClick={handleApproval}
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
                    <div className="p-col-12 p-md-4">
                        <label htmlFor="filterYil">Yıl</label>
                        <InputNumber
                            id="filterYil"
                            value={filters.yil}
                            onValueChange={(e) => onFilterChange(e, 'yil')}
                            useGrouping={false}
                        />
                    </div>

                    <div className="p-col-12 p-md-4">
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
                    selection={selectedBordrolar}
                    onSelectionChange={(e) => setSelectedBordrolar(e.value)}
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
                        selectionMode="multiple"
                        headerStyle={{ width: '3rem' }}
                    ></Column>
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
                        style={{ minWidth: '12rem' }}
                    ></Column>
                </DataTable>
            </Card>

            {/* Detail Dialog */}
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
                                    <p className="p-text-bold p-text-success">{currencyBodyTemplate(selectedBordro, 'netUcret')}</p>
                                </div>
                            </div>
                        </Panel>

                        {selectedBordro.odemeler && selectedBordro.odemeler.length > 0 && (
                            <Panel header="Ödemeler" className="p-mb-3">
                                {selectedBordro.odemeler.map((odeme, index) => (
                                    <div key={index} className="p-grid">
                                        <div className="p-col-8">{odeme.odemeAd}</div>
                                        <div className="p-col-4 p-text-right">
                                            {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(odeme.tutar)}
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
                                        <div className="p-col-4 p-text-right p-text-danger">
                                            -{new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(kesinti.tutar)}
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

            {/* Approval Dialog */}
            <Dialog
                visible={approvalDialog}
                style={{ width: '550px' }}
                header={isApproving ? "Bordro Onaylama" : "Bordro Reddetme"}
                modal
                className="p-fluid"
                footer={approvalDialogFooter}
                onHide={hideApprovalDialog}
            >
                <div>
                    <p>
                        {selectedBordrolar.length === 1 ? (
                            <>
                                <strong>{selectedBordrolar[0]?.personelAd}</strong> personeline ait{' '}
                                <strong>{selectedBordrolar[0]?.yil}/{ayOptions.find(a => a.value === selectedBordrolar[0]?.ay)?.label}</strong>{' '}
                                dönemine ait bordroyu {isApproving ? 'onaylamak' : 'reddetmek'} istediğinizden emin misiniz?
                            </>
                        ) : (
                            <>
                                Seçili <strong>{selectedBordrolar.length} adet bordroyu</strong>{' '}
                                {isApproving ? 'onaylamak' : 'reddetmek'} istediğinizden emin misiniz?
                            </>
                        )}
                    </p>

                    <div className="p-field p-mt-4">
                        <label htmlFor="onayNotu">
                            {isApproving ? "Onay Notu" : "Red Nedeni"} {!isApproving && "*"}
                        </label>
                        <InputTextarea
                            id="onayNotu"
                            value={onayNotu}
                            onChange={(e) => setOnayNotu(e.target.value)}
                            rows={3}
                            cols={20}
                            placeholder={isApproving ? "İsteğe bağlı onay notu..." : "Red nedenini belirtiniz..."}
                        />
                    </div>
                </div>
            </Dialog>
        </div>
    );
};

export default BordroOnayi;
