import React, { useState, useEffect, useRef } from 'react';
import { Card } from 'primereact/card';
import { Toast } from 'primereact/toast';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Avatar } from 'primereact/avatar';
import { Badge } from 'primereact/badge';
import { Panel } from 'primereact/panel';
import { Chip } from 'primereact/chip';
import { Toolbar } from 'primereact/toolbar';
import { ProgressSpinner } from 'primereact/progressspinner';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { TabView, TabPanel } from 'primereact/tabview';
import { OrganizationChart } from 'primereact/organizationchart';
import organizasyonService from '../services/organizasyonService';
import fileUploadService from '../services/fileUploadService';
import authService from '../services/authService';
import yetkiService from '../services/yetkiService';
import './OrganizasyonSemasi.css';

const OrganizasyonSemasi = () => {
    const [organizasyonData, setOrganizasyonData] = useState(null);
    const [departmanIstatistikleri, setDepartmanIstatistikleri] = useState([]);
    const [loading, setLoading] = useState(true);
    const [activeIndex, setActiveIndex] = useState(0);
    const [selectedPersonel, setSelectedPersonel] = useState(null);
    const [detailDialog, setDetailDialog] = useState(false);
    const [currentUser, setCurrentUser] = useState(null);
    const [originalData, setOriginalData] = useState(null);
    const [isAllExpanded, setIsAllExpanded] = useState(false);
    const [chartKey, setChartKey] = useState(0);
    const [selectionKeys, setSelectionKeys] = useState({});
    const [permissions, setPermissions] = useState({
        read: false,
        write: false,
        delete: false,
        update: false
    });
    
    // Pan/Drag functionality state
    const [isPanning, setIsPanning] = useState(false);
    const [startPos, setStartPos] = useState({ x: 0, y: 0 });
    const [scrollStart, setScrollStart] = useState({ x: 0, y: 0 });
    const chartContainerRef = useRef(null);

    const toast = useRef(null);

    // Data manipulation fonksiyonları (üstte tanımla)
    const getCollapsedData = (nodes) => {
        if (!nodes || !Array.isArray(nodes)) return nodes;
        
        // Sadece root level node'ları göster, children'ı kaldır
        return nodes.map(node => ({
            ...node,
            children: [] // Children'ı kaldırarak collapsed görünümü sağla
        }));
    };

    const getExpandedData = (nodes) => {
        // Orijinal data'nın deep copy'sini döndür (tüm children'lar ile)
        return JSON.parse(JSON.stringify(nodes));
    };

    useEffect(() => {
        const user = authService.getUser();
        setCurrentUser(user);
        loadData();
        loadPermissions();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('organizasyon-semasi', 'read'),
                write: yetkiService.hasScreenPermission('organizasyon-semasi', 'write'),
                delete: yetkiService.hasScreenPermission('organizasyon-semasi', 'delete'),
                update: yetkiService.hasScreenPermission('organizasyon-semasi', 'update')
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

    const loadData = async () => {
        setLoading(true);
        try {
            const response = await organizasyonService.getOrganizasyonSemasi();
            // console.log('Organizasyon şeması response:', response);

            if (response.success) {
                const orgData = response.data.organizasyonSemasi;
            // console.log('Orijinal organizasyon verisi:', JSON.stringify(orgData, null, 2));
                setOriginalData(orgData);
                // Başlangıçta tüm veriyi yükle, expansion CSS ile kontrol et
            // console.log('Tüm veri yüklendi:', JSON.stringify(orgData, null, 2));
                setOrganizasyonData(orgData);
                setDepartmanIstatistikleri(response.data.departmanIstatistikleri);
                setIsAllExpanded(false);
            } else {
                throw new Error(response.message || 'Veri getirilemedi');
            }
        } catch (error) {
            // console.error('Organizasyon şeması yükleme hatası:', error);
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Organizasyon şeması yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const nodeTemplate = (node) => {
        const childrenCount = node.children ? node.children.length : 0;
        const kademeColor = getKademeColor(node.pozisyon?.kademe?.seviye);
        
        return (
            <div className="org-node" style={{ borderColor: kademeColor + '33' }}>
                <div className="org-node-content" onClick={() => openPersonelDetail(node)}>
                    <div className="org-avatar-container">
                        {node.fotografUrl ? (
                            <Avatar
                                image={fileUploadService.getAvatarUrl(node.fotografUrl)}
                                size="large"
                                shape="circle"
                                className="org-avatar"
                                style={{ border: `4px solid ${kademeColor}` }}
                                onImageError={(e) => {
            // console.log('Avatar image error:', e);
                                    const target = e.target;
                                    const parent = target?.parentElement;
                                    if (target && parent) {
                                        target.style.display = 'none';
                                        parent.innerHTML = `<div class="pi pi-user" style="font-size: 2rem; color: ${kademeColor}"></div>`;
                                    }
                                }}
                            />
                        ) : (
                            <Avatar
                                label={node.ad.charAt(0) + node.soyad.charAt(0)}
                                size="large"
                                shape="circle"
                                className="org-avatar"
                                style={{ 
                                    backgroundColor: kademeColor, 
                                    color: '#ffffff',
                                    border: `4px solid ${kademeColor}`
                                }}
                            />
                        )}
                        {childrenCount > 0 && (
                            <Badge
                                value={childrenCount}
                                className="org-team-count-badge"
                                style={{ 
                                    position: 'absolute',
                                    bottom: '-12px',
                                    left: '50%',
                                    transform: 'translateX(-50%)',
                                    backgroundColor: '#10b981',
                                    color: '#ffffff',
                                    fontSize: '11px',
                                    fontWeight: '600',
                                    width: '28px',
                                    height: '28px',
                                    border: '3px solid #ffffff',
                                    display: 'flex',
                                    alignItems: 'center',
                                    justifyContent: 'center'
                                }}
                            />
                        )}
                    </div>
                    <div className="org-node-info">
                        <div className="org-node-name">{node.ad} {node.soyad}</div>
                        <div className="org-node-position">{node.pozisyon?.ad || 'Pozisyon Tanımsız'}</div>
                        <div className="org-node-department">
                            <i className="pi pi-building mr-1" style={{ fontSize: '0.7rem', color: '#6b7280' }}></i>
                            {node.pozisyon?.departman?.ad || 'Departman Tanımsız'}
                        </div>
                        <div className="org-node-level">
                            <Badge
                                value={node.pozisyon?.kademe?.ad || 'Kademe Tanımsız'}
                                style={{ 
                                    backgroundColor: kademeColor + '20',
                                    color: kademeColor,
                                    border: `1px solid ${kademeColor}30`
                                }}
                                className="org-kademe-badge"
                            />
                        </div>
                        {node.email && (
                            <div className="org-node-contact mt-2">
                                <i className="pi pi-envelope" style={{ fontSize: '0.6rem', color: '#6b7280', marginRight: '4px' }}></i>
                                <span style={{ fontSize: '0.6rem', color: '#6b7280' }}>{node.email}</span>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        );
    };

    const getKademeColor = (seviye) => {
        const colors = {
            1: '#7c3aed', // Genel Müdür - Mor (En üst seviye)
            2: '#2563eb', // Direktör - Mavi
            3: '#0891b2', // Grup Müdürü - Cyan
            4: '#059669', // Müdür - Yeşil
            5: '#dc2626', // Yönetici - Kırmızı
            6: '#ea580c', // Sorumlu - Turuncu
            7: '#ca8a04', // Kıdemli Uzman - Altın
            8: '#7c2d12', // Uzman - Kahverengi
            9: '#4c1d95'  // Uzman Yardımcısı - Koyu mor
        };
        return colors[seviye] || '#64748b';
    };

    const getKademeLegendItems = () => {
        return [
            { level: 1, name: 'Genel Müdür' },
            { level: 2, name: 'Direktör' },
            { level: 3, name: 'Grup Müdürü' },
            { level: 4, name: 'Müdür' },
            { level: 5, name: 'Yönetici' },
            { level: 6, name: 'Sorumlu' },
            { level: 7, name: 'Kıdemli Uzman' },
            { level: 8, name: 'Uzman' },
            { level: 9, name: 'Uzman Yardımcısı' }
        ];
    };

    const getKademeSeverity = (seviye) => {
        if (seviye <= 2) return 'success';
        if (seviye <= 4) return 'info';
        if (seviye <= 6) return 'warning';
        return 'help';
    };

    const openPersonelDetail = (personel) => {
        setSelectedPersonel(personel);
        setDetailDialog(true);
    };

    const hideDetailDialog = () => {
        setDetailDialog(false);
        setSelectedPersonel(null);
    };

    const toggleAllNodes = () => {
            // console.log('toggleAllNodes çağrıldı. isAllExpanded:', isAllExpanded);
        setIsAllExpanded(!isAllExpanded);
    };

    const getTotalPersonelCount = (data) => {
        if (!data || !Array.isArray(data)) return 0;
        
        let count = 0;
        const countRecursive = (nodes) => {
            nodes.forEach(node => {
                count++;
                if (node.children && node.children.length > 0) {
                    countRecursive(node.children);
                }
            });
        };
        
        countRecursive(data);
        return count;
    };

    const toggleFullscreen = () => {
        const element = document.querySelector('.org-chart-container');
        if (element) {
            if (!document.fullscreenElement) {
                element.requestFullscreen().catch(err => {
            // console.error('Fullscreen error:', err);
                });
            } else {
                document.exitFullscreen();
            }
        }
    };

    const getMaxDepth = (data, currentDepth = 1) => {
        if (!data || !Array.isArray(data)) return 0;
        
        let maxDepth = currentDepth;
        
        data.forEach(node => {
            if (node.children && node.children.length > 0) {
                const childDepth = getMaxDepth(node.children, currentDepth + 1);
                maxDepth = Math.max(maxDepth, childDepth);
            }
        });
        
        return maxDepth;
    };

    const getManagerCount = (data) => {
        if (!data || !Array.isArray(data)) return 0;
        
        let count = 0;
        const countRecursive = (nodes) => {
            nodes.forEach(node => {
                if (node.children && node.children.length > 0) {
                    count++; // This person is a manager
                    countRecursive(node.children);
                }
            });
        };
        
        countRecursive(data);
        return count;
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'Belirtilmemiş';
        return new Date(dateString).toLocaleDateString('tr-TR');
    };

    const getWorkExperience = (startDate) => {
        if (!startDate) return 'Belirtilmemiş';
        
        const start = new Date(startDate);
        const now = new Date();
        const diffTime = Math.abs(now - start);
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        const years = Math.floor(diffDays / 365);
        const months = Math.floor((diffDays % 365) / 30);
        
        if (years > 0) {
            return `${years} yıl ${months} ay`;
        } else {
            return `${months} ay`;
        }
    };

    // Pan/Drag functionality
    const handleMouseDown = (e) => {
        if (e.target.closest('.org-node-content')) {
            return; // Don't start panning when clicking on node content
        }
        
        setIsPanning(true);
        setStartPos({ x: e.clientX, y: e.clientY });
        setScrollStart({
            x: chartContainerRef.current.scrollLeft,
            y: chartContainerRef.current.scrollTop
        });
        e.preventDefault();
        document.body.style.cursor = 'grabbing';
        document.body.style.userSelect = 'none';
    };

    const handleMouseMove = (e) => {
        if (!isPanning) return;

        const dx = e.clientX - startPos.x;
        const dy = e.clientY - startPos.y;

        chartContainerRef.current.scrollLeft = scrollStart.x - dx;
        chartContainerRef.current.scrollTop = scrollStart.y - dy;
    };

    const handleMouseUp = () => {
        if (!isPanning) return;
        
        setIsPanning(false);
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
    };

    // Add event listeners for mouse events on the entire document
    useEffect(() => {
        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
        document.addEventListener('mouseleave', handleMouseUp);

        return () => {
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
            document.removeEventListener('mouseleave', handleMouseUp);
        };
    }, [isPanning, startPos, scrollStart]);

    const leftToolbarTemplate = () => {
        return (
            <React.Fragment>
                <div className="flex align-items-center gap-3">
                    <div className="flex align-items-center gap-2">
                        <i className="pi pi-sitemap text-2xl text-primary"></i>
                        <div>
                            <h5 className="m-0 text-900 font-bold">Organizasyon Şeması</h5>
                            <p className="m-0 text-sm text-600">
                                {organizasyonData ? `${getTotalPersonelCount(organizasyonData)} personel` : 'Yüklüyor...'}
                            </p>
                        </div>
                    </div>
                </div>
            </React.Fragment>
        );
    };

    const rightToolbarTemplate = () => {
        return (
            <React.Fragment>
                <div className="flex align-items-center gap-2">
                    <Button
                        label="Yenile"
                        icon="pi pi-refresh"
                        size="small"
                        outlined
                        onClick={loadData}
                        loading={loading}
                    />
                    <Button
                        label="Tam Ekran"
                        icon="pi pi-expand"
                        size="small"
                        outlined
                        onClick={toggleFullscreen}
                    />
                    <Button
                        label="Yazdır"
                        icon="pi pi-print"
                        size="small"
                        severity="secondary"
                        outlined
                        onClick={() => window.print()}
                    />
                </div>
            </React.Fragment>
        );
    };

    const avatarBodyTemplate = (rowData) => {
        if (rowData.fotografUrl) {
            return (
                <Avatar
                    image={fileUploadService.getAvatarUrl(rowData.fotografUrl)}
                    size="normal"
                    shape="circle"
                    onImageError={(e) => {
            // console.log('Table avatar error:', e);
                    }}
                />
            );
        } else {
            return (
                <Avatar
                    label={rowData.ad.charAt(0) + rowData.soyad.charAt(0)}
                    size="normal"
                    shape="circle"
                    style={{ backgroundColor: getKademeColor(rowData.kademeSeviye), color: '#ffffff' }}
                />
            );
        }
    };

    const kademeDagilimiTemplate = (rowData) => {
        return (
            <div className="kademe-dagilimi">
                {rowData.kademeDagilimi.map((kademe, index) => (
                    <Chip
                        key={index}
                        label={`${kademe.kademeAd}: ${kademe.personelSayisi}`}
                        className="p-mr-1 p-mb-1"
                        style={{ 
                            backgroundColor: getKademeColor(kademe.kademeSeviye), 
                            color: '#ffffff',
                            fontSize: '0.75rem'
                        }}
                    />
                ))}
            </div>
        );
    };

    if (loading) {
        return (
            <div className="organizasyon-semasi-container flex align-items-center justify-content-center" style={{ minHeight: '60vh' }}>
                <div className="text-center">
                    <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="4" />
                    <p className="mt-3 text-600">Organizasyon şeması yükleniyor...</p>
                </div>
            </div>
        );
    }

    if (!permissions.read) {
        return (
            <div className="organizasyon-semasi-container flex align-items-center justify-content-center" style={{ minHeight: '60vh' }}>
                <Card className="text-center">
                    <i className="pi pi-lock text-4xl text-400 mb-3"></i>
                    <h3>Erişim Yetkisi Yok</h3>
                    <p className="text-600">Bu sayfayı görüntülemek için gerekli yetkiniz bulunmamaktadır.</p>
                </Card>
            </div>
        );
    }

    return (
        <div className="organizasyon-semasi-container">
            <Toast ref={toast} />

            {/* Header */}
            <div className="surface-card p-4 border-round shadow-2 mb-4">
                <Toolbar left={leftToolbarTemplate} right={rightToolbarTemplate} />
            </div>

            <TabView activeIndex={activeIndex} onTabChange={(e) => setActiveIndex(e.index)}>
                <TabPanel header="Organizasyon Şeması" leftIcon="pi pi-sitemap">
                    <div className="grid">
                        <div className="col-12">
                            {/* Statistics Cards */}
                            <div className="grid mb-4">
                                <div className="col-12 md:col-3">
                                    <Card className="text-center border-left-3 border-primary">
                                        <div className="text-2xl font-bold text-primary mb-2">
                                            {organizasyonData ? getTotalPersonelCount(organizasyonData) : '0'}
                                        </div>
                                        <div className="text-600 font-medium">Toplam Personel</div>
                                    </Card>
                                </div>
                                <div className="col-12 md:col-3">
                                    <Card className="text-center border-left-3 border-green-500">
                                        <div className="text-2xl font-bold text-green-600 mb-2">
                                            {departmanIstatistikleri.length}
                                        </div>
                                        <div className="text-600 font-medium">Aktif Departman</div>
                                    </Card>
                                </div>
                                <div className="col-12 md:col-3">
                                    <Card className="text-center border-left-3 border-orange-500">
                                        <div className="text-2xl font-bold text-orange-600 mb-2">
                                            {organizasyonData ? getMaxDepth(organizasyonData) : '0'}
                                        </div>
                                        <div className="text-600 font-medium">Organizasyon Derinliği</div>
                                    </Card>
                                </div>
                                <div className="col-12 md:col-3">
                                    <Card className="text-center border-left-3 border-blue-500">
                                        <div className="text-2xl font-bold text-blue-600 mb-2">
                                            {organizasyonData ? getManagerCount(organizasyonData) : '0'}
                                        </div>
                                        <div className="text-600 font-medium">Yönetici Sayısı</div>
                                    </Card>
                                </div>
                            </div>
                        </div>
                        
                        <div className="col-12">
                            <Card>
                                {organizasyonData && organizasyonData.length > 0 ? (
                                    <div 
                                        className="org-chart-container" 
                                        style={{ position: 'relative', cursor: isPanning ? 'grabbing' : 'grab' }}
                                        ref={chartContainerRef}
                                        onMouseDown={handleMouseDown}
                                    >
                                        {/* Hepsini Aç/Kapat Butonu */}
                                        <Button
                                            label={isAllExpanded ? "Hepsini Kapat" : "Hepsini Aç"}
                                            icon={isAllExpanded ? "pi pi-minus" : "pi pi-plus"}
                                            className="p-button-rounded p-button-secondary"
                                            onClick={toggleAllNodes}
                                            style={{
                                                position: 'absolute',
                                                top: '1rem',
                                                right: '1rem',
                                                zIndex: 1001
                                            }}
                                            size="small"
                                        />
                                        
                                        <OrganizationChart
                                            key={chartKey}
                                            value={organizasyonData}
                                            nodeTemplate={nodeTemplate}
                                            className={`company-org-chart ${isAllExpanded ? '' : 'chart-collapsed'}`}
                                        />
                                        
                                        {/* Legend */}
                                        <div className="org-chart-legend" style={{ top: '0.5rem' }}>
                                            <h6 className="mb-3 text-900 font-semibold">Kademe Renk Açıklamaları:</h6>
                                            <div className="flex flex-column gap-2">
                                                {getKademeLegendItems().map(item => (
                                                    <div key={item.level} className="flex align-items-center gap-2 mb-1">
                                                        <div 
                                                            className="w-1rem h-1rem border-round flex-shrink-0"
                                                            style={{ backgroundColor: getKademeColor(item.level) }}
                                                        ></div>
                                                        <span className="text-xs text-600 flex-1">{item.name}</span>
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                    </div>
                                ) : (
                                    <div className="text-center p-6">
                                        <i className="pi pi-info-circle text-4xl text-400 mb-3"></i>
                                        <p className="text-600 text-lg">Organizasyon şeması verisi bulunamadı.</p>
                                    </div>
                                )}
                            </Card>
                        </div>
                    </div>
                </TabPanel>

                <TabPanel header="Departman İstatistikleri" leftIcon="pi pi-chart-bar">
                    <Card>
                        <DataTable
                            value={departmanIstatistikleri}
                            dataKey="departmanId"
                            paginator
                            rows={10}
                            rowsPerPageOptions={[5, 10, 25]}
                            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                            currentPageReportTemplate="{first} - {last} arası, toplam {totalRecords} departman"
                            emptyMessage="Departman bulunamadı."
                            responsiveLayout="scroll"
                        >
                            <Column
                                field="departmanAd"
                                header="Departman"
                                sortable
                                style={{ minWidth: '12rem' }}
                            />
                            <Column
                                field="departmanKod"
                                header="Kod"
                                sortable
                                style={{ minWidth: '8rem' }}
                            />
                            <Column
                                field="toplamPersonel"
                                header="Toplam Personel"
                                sortable
                                style={{ minWidth: '8rem', textAlign: 'center' }}
                                body={(rowData) => (
                                    <div className="flex justify-content-center align-items-center">
                                        <Badge
                                            value={rowData.toplamPersonel}
                                            severity="info"
                                            className="text-base"
                                        />
                                    </div>
                                )}
                            />
                            <Column
                                field="kademeDagilimi"
                                header="Kademe Dağılımı"
                                body={kademeDagilimiTemplate}
                                style={{ minWidth: '20rem' }}
                            />
                        </DataTable>
                    </Card>
                </TabPanel>
            </TabView>

            {/* Personel Detay Dialog */}
            <Dialog
                visible={detailDialog}
                style={{ width: '800px', maxWidth: '95vw' }}
                header={
                    <div className="flex align-items-center gap-3">
                        <i className="pi pi-user text-white text-2xl"></i>
                        <span className="text-2xl font-bold">Personel Profili</span>
                    </div>
                }
                modal
                onHide={hideDetailDialog}
                draggable={false}
                resizable={false}
            >
                {selectedPersonel && (
                    <div className="grid">
                        <div className="col-12">
                            <div className="personel-detail-profile">
                                <div className="flex align-items-center gap-4">
                                    <div className="personel-detail-avatar">
                                        {selectedPersonel.fotografUrl ? (
                                            <Avatar
                                                image={fileUploadService.getAvatarUrl(selectedPersonel.fotografUrl)}
                                                size="xlarge"
                                                shape="circle"
                                                className="shadow-4"
                                                style={{ 
                                                    width: '100px',
                                                    height: '100px',
                                                    border: `4px solid white`,
                                                    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)'
                                                }}
                                                onImageError={(e) => {
            // console.log('Profile popup avatar error:', e);
                                                }}
                                            />
                                        ) : (
                                            <Avatar
                                                label={selectedPersonel.ad.charAt(0) + selectedPersonel.soyad.charAt(0)}
                                                size="xlarge"
                                                shape="circle"
                                                className="shadow-4"
                                                style={{
                                                    backgroundColor: getKademeColor(selectedPersonel.pozisyon?.kademe?.seviye),
                                                    color: '#ffffff',
                                                    width: '100px',
                                                    height: '100px',
                                                    fontSize: '2rem',
                                                    fontWeight: 'bold',
                                                    border: '4px solid white',
                                                    boxShadow: '0 8px 32px rgba(0, 0, 0, 0.12)'
                                                }}
                                            />
                                        )}
                                    </div>
                                    <div className="personel-detail-info flex-1">
                                        <h2 className="text-2xl font-bold text-900 m-0 mb-2 line-height-3">
                                            {selectedPersonel.ad} {selectedPersonel.soyad}
                                        </h2>
                                        <div className="flex align-items-center gap-2 mb-2">
                                            <div className="flex align-items-center justify-content-center bg-blue-100 border-round" style={{ width: '32px', height: '32px' }}>
                                                <i className="pi pi-briefcase text-blue-600 text-sm"></i>
                                            </div>
                                            <span className="text-base text-800 font-semibold">
                                                {selectedPersonel.pozisyon?.ad || 'Pozisyon Tanımsız'}
                                            </span>
                                        </div>
                                        <div className="flex align-items-center gap-2">
                                            <div className="flex align-items-center justify-content-center bg-green-100 border-round" style={{ width: '32px', height: '32px' }}>
                                                <i className="pi pi-building text-green-600 text-sm"></i>
                                            </div>
                                            <span className="text-sm font-medium text-700 bg-green-50 px-3 py-1 border-round-md border-1 border-green-200">
                                                {selectedPersonel.pozisyon?.departman?.ad || 'Departman Tanımsız'}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Information Cards */}
                        <div className="col-12">
                            <div className="grid">
                                <div className="col-12 md:col-6">
                                    <div className="personel-info-card">
                                        <div className="personel-info-icon bg-blue-100">
                                            <i className="pi pi-envelope text-blue-600"></i>
                                        </div>
                                        <div className="flex-1">
                                            <div className="text-600 text-xs mb-1 font-medium">Email Adresi</div>
                                            <div className="font-semibold text-900 text-sm">
                                                {selectedPersonel.email || 'Belirtilmemiş'}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div className="col-12 md:col-6">
                                    <div className="personel-info-card">
                                        <div className="personel-info-icon bg-green-100">
                                            <i className="pi pi-phone text-green-600"></i>
                                        </div>
                                        <div className="flex-1">
                                            <div className="text-600 text-xs mb-1 font-medium">Telefon</div>
                                            <div className="font-semibold text-900 text-sm">
                                                {selectedPersonel.telefon || 'Belirtilmemiş'}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div className="col-12 md:col-6">
                                    <div className="personel-info-card">
                                        <div className="personel-info-icon bg-orange-100">
                                            <i className="pi pi-calendar text-orange-600"></i>
                                        </div>
                                        <div className="flex-1">
                                            <div className="text-600 text-xs mb-1 font-medium">İşe Başlama</div>
                                            <div className="font-semibold text-900 text-sm">
                                                {formatDate(selectedPersonel.iseBaslamaTarihi)}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div className="col-12 md:col-6">
                                    <div className="personel-info-card">
                                        <div className="personel-info-icon bg-indigo-100">
                                            <i className="pi pi-clock text-indigo-600"></i>
                                        </div>
                                        <div className="flex-1">
                                            <div className="text-600 text-xs mb-1 font-medium">İş Deneyimi</div>
                                            <div className="font-semibold text-900 text-sm">
                                                {getWorkExperience(selectedPersonel.iseBaslamaTarihi)}
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {selectedPersonel.yonetici && (
                            <div className="col-12">
                                <div className="manager-section">
                                    <div className="flex align-items-center gap-2 mb-2">
                                        <div className="flex align-items-center justify-content-center bg-indigo-100 border-round" style={{ width: '28px', height: '28px' }}>
                                            <i className="pi pi-user-plus text-indigo-600 text-sm"></i>
                                        </div>
                                        <span className="text-900 font-semibold text-base">Yöneticisi</span>
                                    </div>
                                    <div className="flex align-items-center gap-3 p-2 bg-white border-round-lg shadow-1 border-1 border-200">
                                        {selectedPersonel.yonetici.fotografUrl ? (
                                            <Avatar 
                                                image={fileUploadService.getAvatarUrl(selectedPersonel.yonetici.fotografUrl)}
                                                size="normal"
                                                shape="circle"
                                                style={{ 
                                                    width: '42px',
                                                    height: '42px'
                                                }}
                                                onImageError={(e) => {
            // console.log('Manager avatar error:', e);
                                                }}
                                            />
                                        ) : (
                                            <Avatar 
                                                label={selectedPersonel.yonetici.ad.charAt(0) + selectedPersonel.yonetici.soyad.charAt(0)}
                                                size="normal"
                                                shape="circle"
                                                style={{ 
                                                    backgroundColor: getKademeColor(selectedPersonel.yonetici.kademeSeviye || 1), 
                                                    color: '#ffffff',
                                                    width: '42px',
                                                    height: '42px',
                                                    fontSize: '0.9rem',
                                                    fontWeight: 'bold'
                                                }}
                                            />
                                        )}
                                        <div className="flex-1">
                                            <div className="font-semibold text-900 text-sm mb-1 line-height-2">
                                                {selectedPersonel.yonetici.ad} {selectedPersonel.yonetici.soyad}
                                            </div>
                                            <div className="text-600 flex align-items-center gap-2 mb-1">
                                                <i className="pi pi-briefcase text-xs"></i>
                                                <span className="text-xs font-medium">{selectedPersonel.yonetici.pozisyonAd || 'Pozisyon Tanımsız'}</span>
                                            </div>
                                            <div className="text-600 flex align-items-center gap-2">
                                                <i className="pi pi-building text-xs"></i>
                                                <span className="text-xs font-medium">{selectedPersonel.yonetici.departmanAd || 'Departman Tanımsız'}</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}

                        {selectedPersonel.children && selectedPersonel.children.length > 0 && (
                            <div className="col-12">
                                <div className="surface-card border-round-lg p-4 border-1 border-200">
                                    <div className="flex align-items-center gap-3 mb-4">
                                        <div className="flex align-items-center justify-content-center bg-teal-100 border-round" style={{ width: '36px', height: '36px' }}>
                                            <i className="pi pi-users text-teal-600"></i>
                                        </div>
                                        <span className="text-900 font-semibold text-lg">Bağlı Personeller</span>
                                        <Badge value={selectedPersonel.children.length} className="ml-auto" severity="info" />
                                    </div>
                                    <div className="grid">
                                        {selectedPersonel.children.slice(0, 6).map((altPersonel, index) => (
                                            <div key={index} className="col-12 sm:col-6 lg:col-4">
                                                <div className="flex align-items-center gap-3 p-3 surface-50 border-round-lg hover:surface-100 transition-colors transition-duration-200">
                                                    <Avatar 
                                                        label={altPersonel.ad.charAt(0) + altPersonel.soyad.charAt(0)}
                                                        shape="circle"
                                                        style={{ 
                                                            backgroundColor: getKademeColor(altPersonel.pozisyon?.kademe?.seviye || 9), 
                                                            color: '#ffffff',
                                                            width: '40px',
                                                            height: '40px'
                                                        }}
                                                    />
                                                    <div className="flex-1">
                                                        <div className="font-semibold text-900 text-sm mb-1">{altPersonel.ad} {altPersonel.soyad}</div>
                                                        <div className="text-xs text-600">{altPersonel.pozisyon?.ad}</div>
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                        {selectedPersonel.children.length > 6 && (
                                            <div className="col-12">
                                                <div className="text-center p-3 surface-100 border-round-lg">
                                                    <i className="pi pi-ellipsis-h text-600 mb-2"></i>
                                                    <div className="text-600 text-sm font-medium">
                                                        ve {selectedPersonel.children.length - 6} kişi daha
                                                    </div>
                                                </div>
                                            </div>
                                        )}
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

export default OrganizasyonSemasi;