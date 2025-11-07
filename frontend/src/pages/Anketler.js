import React, { useState, useEffect, useRef } from 'react';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Button } from 'primereact/button';
import { Toast } from 'primereact/toast';
import { Toolbar } from 'primereact/toolbar';
import { Tag } from 'primereact/tag';
import { Card } from 'primereact/card';
import anketService from '../services/anketService';
import yetkiService from '../services/yetkiService';
import { useRouter } from 'next/navigation';

const Anketler = () => {
    const [anketler, setAnketler] = useState([]);
    const [globalFilter, setGlobalFilter] = useState(null);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    const toast = useRef(null);
    const dt = useRef(null);
    const router = useRouter();

    useEffect(() => {
        loadAnketler();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('anketler', 'read'),
                write: yetkiService.hasScreenPermission('anketler', 'write'),
                delete: yetkiService.hasScreenPermission('anketler', 'delete'),
                update: yetkiService.hasScreenPermission('anketler', 'update')
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

    const loadAnketler = async () => {
        setLoading(true);
        try {
            const response = await anketService.getAllAnketler();
            if (response.success) {
                setAnketler(response.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anketler yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const durumBodyTemplate = (rowData) => {
        const badge = anketService.getAnketDurumuBadge(rowData.anketDurumu);
        return <Tag value={badge.label} severity={badge.severity} />;
    };

    const tarihBodyTemplate = (rowData) => {
        return (
            <div>
                <div><strong>Başlangıç:</strong> {anketService.formatDate(rowData.baslangicTarihi)}</div>
                <div><strong>Bitiş:</strong> {anketService.formatDate(rowData.bitisTarihi)}</div>
            </div>
        );
    };

    const soruSayisiBodyTemplate = (rowData) => {
        return rowData.sorular?.length || 0;
    };

    const anonymousBodyTemplate = (rowData) => {
        return rowData.anonymousMu ? <Tag value="Anonim" severity="info" /> : <Tag value="İsimli" severity="secondary" />;
    };

    const durumDegistir = async (anket) => {
        let yeniDurum;
        let mesaj;

        if (anket.anketDurumu === 'Taslak') {
            yeniDurum = 'Aktif';
            mesaj = 'Anket yayınlandı';
        } else if (anket.anketDurumu === 'Aktif') {
            yeniDurum = 'Tamamlandı';
            mesaj = 'Anket yayından kaldırıldı';
        } else {
            yeniDurum = 'Aktif';
            mesaj = 'Anket yeniden yayınlandı';
        }

        try {
            const response = await anketService.updateAnket(anket.id, {
                ...anket,
                anketDurumu: yeniDurum
            });

            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: mesaj,
                    life: 3000
                });
                loadAnketler();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Durum değiştirilirken hata oluştu',
                life: 3000
            });
        }
    };

    const actionBodyTemplate = (rowData) => {
        return (
            <div className="flex gap-2">
                {rowData.anketDurumu === 'Taslak' && permissions.update && (
                    <Button
                        icon="pi pi-send"
                        rounded
                        outlined
                        severity="success"
                        onClick={() => durumDegistir(rowData)}
                        tooltip="Yayınla"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
                {rowData.anketDurumu === 'Aktif' && permissions.update && (
                    <Button
                        icon="pi pi-pause"
                        rounded
                        outlined
                        severity="warning"
                        onClick={() => durumDegistir(rowData)}
                        tooltip="Yayından Kaldır"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
                {rowData.anketDurumu === 'Tamamlandı' && permissions.update && (
                    <Button
                        icon="pi pi-replay"
                        rounded
                        outlined
                        severity="info"
                        onClick={() => durumDegistir(rowData)}
                        tooltip="Yeniden Yayınla"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
                <Button
                    icon="pi pi-chart-bar"
                    rounded
                    outlined
                    className="p-button-info"
                    onClick={() => router.push(`/anket-sonuclari?id=${rowData.id}`)}
                    tooltip="Sonuçları Gör"
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-users"
                    rounded
                    outlined
                    className="p-button-success"
                    onClick={() => router.push(`/anket-atama?id=${rowData.id}`)}
                    tooltip="Atama Yap"
                    tooltipOptions={{ position: 'top' }}
                    disabled={!permissions.write}
                />
                {permissions.delete && (
                    <Button
                        icon="pi pi-trash"
                        rounded
                        outlined
                        severity="danger"
                        onClick={() => confirmDelete(rowData)}
                        tooltip="Sil"
                        tooltipOptions={{ position: 'top' }}
                    />
                )}
            </div>
        );
    };

    const confirmDelete = (anket) => {
        if (confirm(`"${anket.baslik}" adlı anketi silmek istediğinizden emin misiniz?`)) {
            deleteAnket(anket.id);
        }
    };

    const deleteAnket = async (id) => {
        try {
            const response = await anketService.deleteAnket(id);
            if (response.success) {
                toast.current.show({
                    severity: 'success',
                    summary: 'Başarılı',
                    detail: 'Anket silindi',
                    life: 3000
                });
                loadAnketler();
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Anket silinirken hata oluştu',
                life: 3000
            });
        }
    };

    const leftToolbarTemplate = () => {
        return (
            <div className="flex flex-wrap gap-2">
                <Button
                    label="Yeni Anket"
                    icon="pi pi-plus"
                    severity="success"
                    onClick={() => router.push('/anket-olustur')}
                    disabled={!permissions.write}
                />
            </div>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <div className="flex gap-2">
                <span className="p-input-icon-left">
                    <i className="pi pi-search" />
                    <input
                        type="search"
                        className="p-inputtext p-component"
                        placeholder="Ara..."
                        onInput={(e) => setGlobalFilter(e.target.value)}
                    />
                </span>
                <Button
                    label="Yenile"
                    icon="pi pi-refresh"
                    className="p-button-help"
                    onClick={loadAnketler}
                />
            </div>
        );
    };

    if (!permissions.read) {
        return (
            <div className="flex align-items-center justify-content-center" style={{ minHeight: '400px' }}>
                <Card>
                    <div className="text-center">
                        <i className="pi pi-lock" style={{ fontSize: '3rem', color: 'var(--primary-color)' }}></i>
                        <h3>Yetkiniz Yok</h3>
                        <p>Bu sayfayı görüntülemek için yetkiniz bulunmamaktadır.</p>
                    </div>
                </Card>
            </div>
        );
    }

    return (
        <div className="grid">
            <div className="col-12">
                <Card title="Anket Yönetimi">
                    <Toast ref={toast} />
                    <Toolbar className="mb-4" left={leftToolbarTemplate} right={rightToolbarTemplate}></Toolbar>

                    <DataTable
                        ref={dt}
                        value={anketler}
                        selection={null}
                        dataKey="id"
                        paginator
                        rows={10}
                        rowsPerPageOptions={[5, 10, 25]}
                        className="datatable-responsive"
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        currentPageReportTemplate="{first} - {last} / {totalRecords} anket"
                        globalFilter={globalFilter}
                        emptyMessage="Anket bulunamadı"
                        loading={loading}
                        responsiveLayout="scroll"
                    >
                        <Column field="baslik" header="Başlık" sortable style={{ minWidth: '200px' }}></Column>
                        <Column body={durumBodyTemplate} header="Durum" sortable style={{ minWidth: '120px' }}></Column>
                        <Column body={tarihBodyTemplate} header="Tarihler" style={{ minWidth: '180px' }}></Column>
                        <Column body={soruSayisiBodyTemplate} header="Soru Sayısı" sortable style={{ minWidth: '120px' }}></Column>
                        <Column body={anonymousBodyTemplate} header="Tip" style={{ minWidth: '100px' }}></Column>
                        <Column field="olusturanPersonel.ad" header="Oluşturan" sortable style={{ minWidth: '150px' }}></Column>
                        <Column body={actionBodyTemplate} header="İşlemler" exportable={false} style={{ minWidth: '200px' }}></Column>
                    </DataTable>
                </Card>
            </div>
        </div>
    );
};

export default Anketler;
