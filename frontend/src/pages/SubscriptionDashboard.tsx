import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import subscriptionService, {
  type CurrentSubscription,
  type UsageStats,
  type Invoice,
} from '../services/subscriptionService';

const SubscriptionDashboard = () => {
  const [subscription, setSubscription] = useState<CurrentSubscription | null>(null);
  const [usage, setUsage] = useState<UsageStats | null>(null);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [subData, usageData, invoiceData] = await Promise.all([
        subscriptionService.getCurrentSubscription(),
        subscriptionService.getUsageStats(),
        subscriptionService.getBillingHistory(),
      ]);
      setSubscription(subData);
      setUsage(usageData);
      setInvoices(invoiceData);
    } catch (err: any) {
      console.error('Error loading subscription data:', err);
      if (err.response?.status === 404) {
        // No subscription found, redirect to pricing
        navigate('/pricing');
      }
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('tr-TR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const getStatusBadge = (status: number) => {
    const statuses: Record<number, { label: string; color: string }> = {
      1: { label: 'Deneme', color: 'bg-blue-100 text-blue-800' },
      2: { label: 'Aktif', color: 'bg-green-100 text-green-800' },
      3: { label: 'Gecikmiş', color: 'bg-yellow-100 text-yellow-800' },
      4: { label: 'Askıda', color: 'bg-gray-100 text-gray-800' },
      5: { label: 'İptal', color: 'bg-red-100 text-red-800' },
      6: { label: 'Süresi Dolmuş', color: 'bg-red-100 text-red-800' },
    };
    const status_info = statuses[status] || statuses[2];
    return (
      <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${status_info.color}`}>
        {status_info.label}
      </span>
    );
  };

  const getInvoiceStatusBadge = (status: number) => {
    const statuses: Record<number, { label: string; color: string }> = {
      0: { label: 'Taslak', color: 'bg-gray-100 text-gray-800' },
      1: { label: 'Gönderildi', color: 'bg-blue-100 text-blue-800' },
      2: { label: 'Ödendi', color: 'bg-green-100 text-green-800' },
      3: { label: 'Gecikmiş', color: 'bg-red-100 text-red-800' },
      4: { label: 'İptal', color: 'bg-gray-100 text-gray-800' },
      5: { label: 'İade', color: 'bg-yellow-100 text-yellow-800' },
    };
    const status_info = statuses[status] || statuses[0];
    return (
      <span className={`inline-flex items-center px-2 py-1 rounded text-xs font-medium ${status_info.color}`}>
        {status_info.label}
      </span>
    );
  };

  const getUsagePercentageColor = (percentage: number) => {
    if (percentage >= 90) return 'bg-red-600';
    if (percentage >= 75) return 'bg-yellow-600';
    return 'bg-green-600';
  };

  const handleCancelSubscription = async () => {
    if (!cancelReason.trim()) {
      alert('Lütfen iptal nedeninizi belirtin');
      return;
    }

    try {
      await subscriptionService.cancelSubscription(cancelReason);
      setShowCancelModal(false);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'İptal işlemi başarısız');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (!subscription || !usage) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Aktif Abonelik Bulunamadı</h2>
          <button
            onClick={() => navigate('/pricing')}
            className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
          >
            Planları Görüntüle
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Abonelik Yönetimi</h1>
          <p className="mt-2 text-gray-600">Mevcut planınızı ve kullanım durumunuzu yönetin</p>
        </div>

        {/* Current Plan Card */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <div className="flex items-start justify-between mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">{subscription.plan.name}</h2>
              <p className="text-gray-600 mt-1">{subscription.plan.description}</p>
            </div>
            {getStatusBadge(subscription.status)}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
            <div>
              <p className="text-sm text-gray-600">Fiyat</p>
              <p className="text-2xl font-bold text-gray-900">
                {formatPrice(
                  subscription.billingCycle === 2
                    ? subscription.plan.annualPrice
                    : subscription.plan.monthlyPrice
                )}
              </p>
              <p className="text-sm text-gray-600">
                / {subscription.billingCycle === 2 ? 'Yıl' : 'Ay'}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-600">Başlangıç Tarihi</p>
              <p className="text-lg font-semibold text-gray-900">
                {formatDate(subscription.startDate)}
              </p>
            </div>

            {subscription.trialEndDate && subscription.isTrialing && (
              <div>
                <p className="text-sm text-gray-600">Deneme Bitiş</p>
                <p className="text-lg font-semibold text-indigo-600">
                  {formatDate(subscription.trialEndDate)}
                </p>
                <p className="text-sm text-indigo-600">
                  {subscription.daysUntilExpiry} gün kaldı
                </p>
              </div>
            )}

            {subscription.nextBillingDate && !subscription.isTrialing && (
              <div>
                <p className="text-sm text-gray-600">Sonraki Fatura</p>
                <p className="text-lg font-semibold text-gray-900">
                  {formatDate(subscription.nextBillingDate)}
                </p>
              </div>
            )}
          </div>

          <div className="flex flex-wrap gap-4">
            <button
              onClick={() => navigate('/pricing')}
              className="px-6 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition"
            >
              Plan Değiştir
            </button>
            {subscription.status === 2 && (
              <button
                onClick={() => setShowCancelModal(true)}
                className="px-6 py-2 bg-white text-red-600 border border-red-600 rounded-lg hover:bg-red-50 transition"
              >
                Aboneliği İptal Et
              </button>
            )}
          </div>
        </div>

        {/* Usage Stats */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-8">
          <h3 className="text-xl font-bold text-gray-900 mb-6">Kullanım İstatistikleri</h3>
          
          <div className="space-y-6">
            {/* Staff Usage */}
            <div>
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm font-medium text-gray-700">Personel</span>
                <span className="text-sm text-gray-600">
                  {usage.staff.current} / {usage.staff.max === -1 ? '∞' : usage.staff.max}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${getUsagePercentageColor(usage.staff.percentage)}`}
                  style={{ width: `${Math.min(usage.staff.percentage, 100)}%` }}
                ></div>
              </div>
            </div>

            {/* Appointments Usage */}
            <div>
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm font-medium text-gray-700">Aylık Randevular</span>
                <span className="text-sm text-gray-600">
                  {usage.appointments.current} / {usage.appointments.max === -1 ? '∞' : usage.appointments.max}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${getUsagePercentageColor(usage.appointments.percentage)}`}
                  style={{ width: `${Math.min(usage.appointments.percentage, 100)}%` }}
                ></div>
              </div>
            </div>

            {/* Locations Usage */}
            <div>
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm font-medium text-gray-700">Şubeler</span>
                <span className="text-sm text-gray-600">
                  {usage.locations.current} / {usage.locations.max === -1 ? '∞' : usage.locations.max}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${getUsagePercentageColor(usage.locations.percentage)}`}
                  style={{ width: `${Math.min(usage.locations.percentage, 100)}%` }}
                ></div>
              </div>
            </div>

            {/* Services Usage */}
            <div>
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm font-medium text-gray-700">Hizmetler</span>
                <span className="text-sm text-gray-600">
                  {usage.services.current} / {usage.services.max === -1 ? '∞' : usage.services.max}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${getUsagePercentageColor(usage.services.percentage)}`}
                  style={{ width: `${Math.min(usage.services.percentage, 100)}%` }}
                ></div>
              </div>
            </div>
          </div>
        </div>

        {/* Billing History */}
        <div className="bg-white rounded-lg shadow-md p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6">Fatura Geçmişi</h3>
          
          {invoices.length === 0 ? (
            <p className="text-gray-600 text-center py-8">Henüz fatura bulunmuyor</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Fatura No
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tarih
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Vade
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Tutar
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Durum
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {invoices.map((invoice) => (
                    <tr key={invoice.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {invoice.invoiceNumber}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {formatDate(invoice.invoiceDate)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {formatDate(invoice.dueDate)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                        {formatPrice(invoice.totalAmount)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getInvoiceStatusBadge(invoice.status)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {/* Cancel Modal */}
      {showCancelModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-xl font-bold text-gray-900 mb-4">Aboneliği İptal Et</h3>
            <p className="text-gray-600 mb-4">
              Aboneliğinizi iptal etmek istediğinizden emin misiniz? 
              Mevcut dönem sonuna kadar hizmetinizi kullanmaya devam edebilirsiniz.
            </p>
            
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                İptal Nedeni
              </label>
              <textarea
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                rows={4}
                placeholder="Lütfen iptal nedeninizi belirtin..."
              />
            </div>

            <div className="flex gap-4">
              <button
                onClick={handleCancelSubscription}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                İptal Et
              </button>
              <button
                onClick={() => setShowCancelModal(false)}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Vazgeç
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default SubscriptionDashboard;
