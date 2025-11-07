import React, { useState, useEffect, useRef } from 'react';
import { Button } from 'primereact/button';
import { InputNumber } from 'primereact/inputnumber';
import { Toast } from 'primereact/toast';
import { Card } from 'primereact/card';
import { Dropdown } from 'primereact/dropdown';
import { Panel } from 'primereact/panel';
import { Chart } from 'primereact/chart';
import bordroService from '../services/bordroService';
import yetkiService from '../services/yetkiService';

const BordroDashboard = () => {
    const [filters, setFilters] = useState({
        yil: new Date().getFullYear(),
        ay: new Date().getMonth() + 1
    });
    const [ozet, setOzet] = useState(null);
    const [departmanDagilim, setDepartmanDagilim] = useState([]);
    const [loading, setLoading] = useState(false);
    const [permissions, setPermissions] = useState({
        read: false
    });
    const toast = useRef(null);

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
        loadPermissions();
        loadDashboardData();
    }, []);

    const loadPermissions = async () => {
        try {
            await yetkiService.loadUserPermissions();
            setPermissions({
                read: yetkiService.hasScreenPermission('bordro-dashboard', 'read')
            });
        } catch (error) {
            // console.error('Permission loading error:', error);
            setPermissions({
                read: false
            });
        }
    };

    const loadDashboardData = async () => {
        setLoading(true);
        try {
            const [ozetResponse, departmanResponse] = await Promise.all([
                bordroService.getOzet(filters.yil, filters.ay),
                bordroService.getDepartmanDagilim(filters.yil, filters.ay)
            ]);

            if (ozetResponse.success) {
                setOzet(ozetResponse.data);
            }

            if (departmanResponse.success) {
                setDepartmanDagilim(departmanResponse.data);
            }
        } catch (error) {
            toast.current.show({
                severity: 'error',
                summary: 'Hata',
                detail: error.message || 'Veriler yüklenirken hata oluştu',
                life: 3000
            });
        } finally {
            setLoading(false);
        }
    };

    const onFilterChange = (e, name) => {
        const val = (e.target && e.target.value) !== undefined ? e.target.value : e.value;
        let _filters = { ...filters };
        _filters[`${name}`] = val;
        setFilters(_filters);
    };

    const applyFilters = () => {
        loadDashboardData();
    };

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(value || 0);
    };

    // Chart Data
    const getDepartmanChartData = () => {
        if (!departmanDagilim || departmanDagilim.length === 0) return null;

        const labels = departmanDagilim.map(d => d.departmanAd || 'Diğer');
        const brutData = departmanDagilim.map(d => d.toplamBrutUcret);
        const netData = departmanDagilim.map(d => d.toplamNetUcret);

        return {
            labels: labels,
            datasets: [
                {
                    label: 'Brüt Ücret',
                    backgroundColor: '#42A5F5',
                    data: brutData
                },
                {
                    label: 'Net Ücret',
                    backgroundColor: '#66BB6A',
                    data: netData
                }
            ]
        };
    };

    const getDepartmanPersonelChartData = () => {
        if (!departmanDagilim || departmanDagilim.length === 0) return null;

        const labels = departmanDagilim.map(d => d.departmanAd || 'Diğer');
        const personelSayisi = departmanDagilim.map(d => d.personelSayisi);

        // Generate dynamic colors
        const backgroundColors = departmanDagilim.map((_, index) => {
            const colors = ['#42A5F5', '#66BB6A', '#FFA726', '#AB47BC', '#26A69A', '#EC407A'];
            return colors[index % colors.length];
        });

        return {
            labels: labels,
            datasets: [
                {
                    data: personelSayisi,
                    backgroundColor: backgroundColors
                }
            ]
        };
    };

    const chartOptions = {
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            legend: {
                labels: {
                    color: '#495057'
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    color: '#495057'
                },
                grid: {
                    color: '#ebedef'
                }
            },
            y: {
                ticks: {
                    color: '#495057',
                    callback: function(value) {
                        return new Intl.NumberFormat('tr-TR').format(value) + ' ₺';
                    }
                },
                grid: {
                    color: '#ebedef'
                }
            }
        }
    };

    const pieChartOptions = {
        plugins: {
            legend: {
                labels: {
                    color: '#495057'
                }
            }
        }
    };

    return (
        <div className="datatable-crud-demo">
            <Toast ref={toast} />

            <Card className="p-mb-3">
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

                    <div className="p-col-12 p-md-2">
                        <label>&nbsp;</label>
                        <Button
                            label="Yenile"
                            icon="pi pi-refresh"
                            onClick={applyFilters}
                            loading={loading}
                        />
                    </div>
                </div>
            </Card>

            {ozet && (
                <>
                    <div className="p-grid">
                        <div className="p-col-12 p-md-3">
                            <Card>
                                <div className="p-d-flex p-flex-column p-ai-center">
                                    <i className="pi pi-users" style={{ fontSize: '2.5rem', color: '#42A5F5' }}></i>
                                    <h3 className="p-mt-2 p-mb-1">{ozet.toplamPersonel}</h3>
                                    <span className="p-text-secondary">Toplam Personel</span>
                                </div>
                            </Card>
                        </div>

                        <div className="p-col-12 p-md-3">
                            <Card>
                                <div className="p-d-flex p-flex-column p-ai-center">
                                    <i className="pi pi-money-bill" style={{ fontSize: '2.5rem', color: '#FFA726' }}></i>
                                    <h3 className="p-mt-2 p-mb-1">{formatCurrency(ozet.toplamBrutUcret)}</h3>
                                    <span className="p-text-secondary">Toplam Brüt Ücret</span>
                                </div>
                            </Card>
                        </div>

                        <div className="p-col-12 p-md-3">
                            <Card>
                                <div className="p-d-flex p-flex-column p-ai-center">
                                    <i className="pi pi-wallet" style={{ fontSize: '2.5rem', color: '#66BB6A' }}></i>
                                    <h3 className="p-mt-2 p-mb-1">{formatCurrency(ozet.toplamNetUcret)}</h3>
                                    <span className="p-text-secondary">Toplam Net Ücret</span>
                                </div>
                            </Card>
                        </div>

                        <div className="p-col-12 p-md-3">
                            <Card>
                                <div className="p-d-flex p-flex-column p-ai-center">
                                    <i className="pi pi-chart-line" style={{ fontSize: '2.5rem', color: '#AB47BC' }}></i>
                                    <h3 className="p-mt-2 p-mb-1">{formatCurrency(ozet.ortalamaNetUcret)}</h3>
                                    <span className="p-text-secondary">Ortalama Net Ücret</span>
                                </div>
                            </Card>
                        </div>
                    </div>

                    <div className="p-grid p-mt-3">
                        <div className="p-col-12 p-lg-8">
                            <Card title="Departman Bazlı Ücret Dağılımı">
                                {getDepartmanChartData() && (
                                    <Chart
                                        type="bar"
                                        data={getDepartmanChartData()}
                                        options={chartOptions}
                                        style={{ height: '400px' }}
                                    />
                                )}
                            </Card>
                        </div>

                        <div className="p-col-12 p-lg-4">
                            <Card title="Departman Personel Dağılımı">
                                {getDepartmanPersonelChartData() && (
                                    <Chart
                                        type="doughnut"
                                        data={getDepartmanPersonelChartData()}
                                        options={pieChartOptions}
                                        style={{ height: '400px' }}
                                    />
                                )}
                            </Card>
                        </div>
                    </div>

                    {departmanDagilim.length > 0 && (
                        <div className="p-grid p-mt-3">
                            <div className="p-col-12">
                                <Panel header="Departman Detayları">
                                    <div className="p-grid">
                                        {departmanDagilim.map((dept, index) => (
                                            <div key={index} className="p-col-12 p-md-6 p-lg-4">
                                                <Card>
                                                    <h6 className="p-mb-3">{dept.departmanAd || 'Diğer'}</h6>
                                                    <div className="p-d-flex p-jc-between p-mb-2">
                                                        <span>Personel:</span>
                                                        <strong>{dept.personelSayisi}</strong>
                                                    </div>
                                                    <div className="p-d-flex p-jc-between p-mb-2">
                                                        <span>Toplam Brüt:</span>
                                                        <strong>{formatCurrency(dept.toplamBrutUcret)}</strong>
                                                    </div>
                                                    <div className="p-d-flex p-jc-between p-mb-2">
                                                        <span>Toplam Net:</span>
                                                        <strong className="p-text-success">{formatCurrency(dept.toplamNetUcret)}</strong>
                                                    </div>
                                                    <div className="p-d-flex p-jc-between">
                                                        <span>Ortalama Net:</span>
                                                        <strong>{formatCurrency(dept.ortalamaNetUcret)}</strong>
                                                    </div>
                                                </Card>
                                            </div>
                                        ))}
                                    </div>
                                </Panel>
                            </div>
                        </div>
                    )}
                </>
            )}

            {!ozet && !loading && (
                <Card>
                    <div className="p-text-center p-p-5">
                        <i className="pi pi-chart-bar" style={{ fontSize: '3rem', color: '#dee2e6' }}></i>
                        <h5 className="p-mt-3">Dashboard Yükleniyor</h5>
                        <p className="p-text-secondary">Bordro istatistiklerini görüntülemek için yukarıdan dönem seçin.</p>
                    </div>
                </Card>
            )}
        </div>
    );
};

export default BordroDashboard;
