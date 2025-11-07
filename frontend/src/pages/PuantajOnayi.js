import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { InputTextarea } from 'primereact/inputtextarea';
import { confirmDialog } from 'primereact/confirmdialog';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';
import { Divider } from 'primereact/divider';
import puantajService from '../services/puantajService';
import yetkiService from '../services/yetkiService';

const PuantajOnayi = () => {
    const [puantajlar, setPuantajlar] = useState([]);
    const [detailDialog, setDetailDialog] = useState(false);
    const [approvalDialog, setApprovalDialog] = useState(false);
    const [selectedPuantaj, setSelectedPuantaj] = useState(null);
    const [onayNotu, setOnayNotu] = useState('');
    const [isApproving, setIsApproving] = useState(true);
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
        loadPuantajlar();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('puantaj-onayi', 'read'),
                approve: yetkiService.hasScreenPermission('puantaj-onayi', 'update')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false,
                approve: false
            });
        }
    };

    const loadPuantajlar = async () => {
        setLoading(true);
        try {
            const response = await puantajService.getAll();
            if (response.success) {
                setPuantajlar(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Puantajlar yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const showDetail = (puantaj) => {
        setSelectedPuantaj(puantaj);
        setDetailDialog(true);
    };

    const hideDetailDialog = () => {
        setDetailDialog(false);
        setSelectedPuantaj(null);
    };

    const confirmApproval = (puantaj, isApprove) => {
        setSelectedPuantaj(puantaj);
        setIsApproving(isApprove);
        setOnayNotu('');
        setApprovalDialog(true);
    };

    const hideApprovalDialog = () => {
        setApprovalDialog(false);
        setSelectedPuantaj(null);
        setOnayNotu('');
    };

    const handleApproval = async () => {
        if (!selectedPuantaj || processing) return;

        setProcessing(true);
        try {
            const data = {
                onayNotu: onayNotu
            };

            let response;
            if (isApproving) {
                response = await puantajService.onayla(selectedPuantaj.id, data);
            } else {
                response = await puantajService.reddet(selectedPuantaj.id, data);
            }

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: response.message,
                    life: 3000
                });
                loadPuantajlar();
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

        return <Tag value={rowData.onayDurumu} severity={getSeverity(rowData.onayDurumu)} />;
    };

    const ayBodyTemplate = (rowData) => {
        const ay = ayOptions.find(a => a.value === rowData.ay);
        return ay ? ay.label : rowData.ay;
    };

    const toplamCalismaSaatiBodyTemplate = (rowData) => {
        const toplamSaat = (rowData.mesaiSaati || 0) + (rowData.fazlaMesai || 0);
        return `${toplamSaat.toFixed(1)} saat`;
    };

    const actionBodyTemplate = (rowData) => {
        const isPending = rowData.onayDurumu === 'Beklemede';

        return (
            <React.Fragment>
                <Button
                    icon="pi pi-eye"
                    className="p-button-rounded p-button-info p-mr-2"
                    onClick={() => showDetail(rowData)}
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
            <h5 className="p-m-0">Puantaj Onay İşlemleri</h5>
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

            <Card>
                <Toolbar
                    className="p-mb-4"
                    right={rightToolbarTemplate}
                ></Toolbar>

                <DataTable
                    ref={dt}
                    value={puantajlar}
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
                        field="calismaGunu"
                        header="Çalışma Günü"
                        sortable
                        style={{ minWidth: '8rem' }}
                    ></Column>
                    <Column
                        header="Toplam Çalışma"
                        body={toplamCalismaSaatiBodyTemplate}
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
                style={{ width: '600px' }}
                header="Puantaj Detayları"
                modal
                className="p-fluid"
                onHide={hideDetailDialog}
            >
                {selectedPuantaj && (
                    <div>
                        <div className="p-grid">
                            <div className="p-col-6">
                                <strong>Personel:</strong>
                                <p>{selectedPuantaj.personelAd}</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Dönem:</strong>
                                <p>{selectedPuantaj.yil} / {ayOptions.find(a => a.value === selectedPuantaj.ay)?.label}</p>
                            </div>
                        </div>

                        <Divider />

                        <h6>Çalışma Bilgileri</h6>
                        <div className="p-grid">
                            <div className="p-col-6">
                                <strong>Çalışma Günü:</strong>
                                <p>{selectedPuantaj.calismaGunu} gün</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Mesai Saati:</strong>
                                <p>{selectedPuantaj.mesaiSaati} saat</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Fazla Mesai:</strong>
                                <p>{selectedPuantaj.fazlaMesai} saat</p>
                            </div>
                        </div>

                        <Divider />

                        <h6>İzin Bilgileri</h6>
                        <div className="p-grid">
                            <div className="p-col-6">
                                <strong>Haftalık Tatil:</strong>
                                <p>{selectedPuantaj.haftalikTatil} gün</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Yıllık İzin:</strong>
                                <p>{selectedPuantaj.yillikIzin} gün</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Hastalık İzni:</strong>
                                <p>{selectedPuantaj.hastalikIzni} gün</p>
                            </div>
                            <div className="p-col-6">
                                <strong>Mazeret İzni:</strong>
                                <p>{selectedPuantaj.mazeretIzni} gün</p>
                            </div>
                        </div>

                        {selectedPuantaj.aciklama && (
                            <>
                                <Divider />
                                <h6>Açıklama</h6>
                                <p>{selectedPuantaj.aciklama}</p>
                            </>
                        )}

                        <Divider />

                        <div className="p-grid">
                            <div className="p-col-12">
                                <strong>Onay Durumu:</strong>
                                <div className="p-mt-2">
                                    {onayDurumuBodyTemplate(selectedPuantaj)}
                                </div>
                            </div>
                        </div>

                        {selectedPuantaj.onayNotu && (
                            <div className="p-grid p-mt-2">
                                <div className="p-col-12">
                                    <strong>Onay Notu:</strong>
                                    <p>{selectedPuantaj.onayNotu}</p>
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </Dialog>

            {/* Approval Dialog */}
            <Dialog
                visible={approvalDialog}
                style={{ width: '450px' }}
                header={isApproving ? "Puantaj Onaylama" : "Puantaj Reddetme"}
                modal
                className="p-fluid"
                footer={approvalDialogFooter}
                onHide={hideApprovalDialog}
            >
                {selectedPuantaj && (
                    <div>
                        <p>
                            <strong>{selectedPuantaj.personelAd}</strong> personeline ait{' '}
                            <strong>{selectedPuantaj.yil}/{ayOptions.find(a => a.value === selectedPuantaj.ay)?.label}</strong>{' '}
                            dönemine ait puantajı {isApproving ? 'onaylamak' : 'reddetmek'} istediğinizden emin misiniz?
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
                )}
            </Dialog>
        </div>
    );
};

export default PuantajOnayi;
