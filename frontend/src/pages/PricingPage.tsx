import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import subscriptionService, { type SubscriptionPlan } from '../services/subscriptionService';

const PricingPage = () => {
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedBilling, setSelectedBilling] = useState<'monthly' | 'annual'>('monthly');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  // Simple SVG Icons
  const CheckIcon = () => (
    <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
    </svg>
  );

  const XMarkIcon = () => (
    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
    </svg>
  );

  const SparklesIcon = () => (
    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
    </svg>
  );

  const RocketIcon = () => (
    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
    </svg>
  );

  useEffect(() => {
    loadPlans();
  }, []);

  const loadPlans = async () => {
    try {
      setLoading(true);
      const data = await subscriptionService.getPlans();
      setPlans(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Planlar yüklenirken hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  const parseFeatures = (featuresJson: string): string[] => {
    try {
      return JSON.parse(featuresJson);
    } catch {
      return [];
    }
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const getPrice = (plan: SubscriptionPlan) => {
    return selectedBilling === 'annual' ? plan.annualPrice : plan.monthlyPrice;
  };

  const handleStartTrial = async (planId: string) => {
    try {
      await subscriptionService.createTrialSubscription(planId);
      navigate('/dashboard/subscription');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Deneme sürümü başlatılırken hata oluştu');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600">{error}</p>
          <button
            onClick={loadPlans}
            className="mt-4 px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
          >
            Tekrar Dene
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-extrabold text-gray-900 sm:text-5xl">
            Size Uygun Planı Seçin
          </h1>
          <p className="mt-4 text-xl text-gray-600">
            14 günlük ücretsiz deneme ile başlayın. Kredi kartı gerekmez.
          </p>
        </div>

        {/* Billing Toggle */}
        <div className="flex justify-center mb-12">
          <div className="relative bg-gray-100 p-1 rounded-lg inline-flex">
            <button
              onClick={() => setSelectedBilling('monthly')}
              className={`
                px-6 py-2 rounded-md font-medium transition-all
                ${selectedBilling === 'monthly'
                  ? 'bg-white text-indigo-600 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
                }
              `}
            >
              Aylık
            </button>
            <button
              onClick={() => setSelectedBilling('annual')}
              className={`
                px-6 py-2 rounded-md font-medium transition-all
                ${selectedBilling === 'annual'
                  ? 'bg-white text-indigo-600 shadow-sm'
                  : 'text-gray-600 hover:text-gray-900'
                }
              `}
            >
              Yıllık
              <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">
                %20 İndirim
              </span>
            </button>
          </div>
        </div>

        {/* Pricing Cards */}
        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3 lg:gap-x-8">
          {plans.map((plan) => {
            const features = parseFeatures(plan.features);
            const price = getPrice(plan);
            const isPopular = plan.isPopular;

            return (
              <div
                key={plan.id}
                className={`
                  relative rounded-2xl border-2 p-8 transition-all hover:shadow-2xl
                  ${isPopular
                    ? 'border-indigo-600 shadow-xl scale-105'
                    : 'border-gray-200 hover:border-indigo-300'
                  }
                `}
              >
                {/* Popular Badge */}
                {isPopular && (
                  <div className="absolute -top-5 left-0 right-0 flex justify-center">
                    <span className="inline-flex items-center px-4 py-1 rounded-full text-sm font-semibold bg-indigo-600 text-white">
                      <span className="mr-1"><SparklesIcon /></span>
                      En Popüler
                    </span>
                  </div>
                )}

                {/* Plan Badge */}
                {plan.badge && (
                  <div className="mb-4">
                    <span
                      className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
                      style={{
                        backgroundColor: plan.color ? `${plan.color}20` : '#EEF2FF',
                        color: plan.color || '#4F46E5',
                      }}
                    >
                      {plan.badge}
                    </span>
                  </div>
                )}

                {/* Plan Name */}
                <h3 className="text-2xl font-bold text-gray-900">{plan.name}</h3>

                {/* Description */}
                <p className="mt-4 text-gray-600">{plan.description}</p>

                {/* Price */}
                <div className="mt-6">
                  <div className="flex items-baseline">
                    <span className="text-5xl font-extrabold text-gray-900">
                      {formatPrice(price)}
                    </span>
                    <span className="ml-2 text-gray-600">
                      / {selectedBilling === 'annual' ? 'yıl' : 'ay'}
                    </span>
                  </div>
                  {plan.trialDays > 0 && (
                    <p className="mt-2 text-sm text-indigo-600 font-medium">
                      {plan.trialDays} gün ücretsiz deneme
                    </p>
                  )}
                </div>

                {/* CTA Button */}
                <button
                  onClick={() => handleStartTrial(plan.id)}
                  className={`
                    mt-8 w-full py-3 px-6 rounded-lg font-semibold transition-all
                    ${isPopular
                      ? 'bg-indigo-600 text-white hover:bg-indigo-700 shadow-md hover:shadow-lg'
                      : 'bg-gray-100 text-gray-900 hover:bg-gray-200'
                    }
                  `}
                >
                  <span className="flex items-center justify-center">
                    <span className="mr-2"><RocketIcon /></span>
                    Ücretsiz Dene
                  </span>
                </button>

                {/* Features */}
                <div className="mt-8 space-y-4">
                  <h4 className="text-sm font-semibold text-gray-900 uppercase tracking-wide">
                    Özellikler
                  </h4>
                  <ul className="space-y-3">
                    {features.map((feature, index) => (
                      <li key={index} className="flex items-start">
                        <span className="text-green-500 mr-3 flex-shrink-0 mt-0.5"><CheckIcon /></span>
                        <span className="text-gray-600">{feature}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                {/* Limits */}
                <div className="mt-6 pt-6 border-t border-gray-200">
                  <h4 className="text-sm font-semibold text-gray-900 mb-3">
                    Kullanım Limitleri
                  </h4>
                  <div className="space-y-2 text-sm text-gray-600">
                    <div className="flex justify-between">
                      <span>Personel:</span>
                      <span className="font-medium">
                        {plan.maxStaff === -1 ? 'Sınırsız' : plan.maxStaff}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span>Aylık Randevu:</span>
                      <span className="font-medium">
                        {plan.maxAppointmentsPerMonth === -1 ? 'Sınırsız' : plan.maxAppointmentsPerMonth}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span>Şube:</span>
                      <span className="font-medium">
                        {plan.maxLocations === -1 ? 'Sınırsız' : plan.maxLocations}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span>Hizmet:</span>
                      <span className="font-medium">
                        {plan.maxServices === -1 ? 'Sınırsız' : plan.maxServices}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Advanced Features */}
                <div className="mt-6 pt-6 border-t border-gray-200">
                  <h4 className="text-sm font-semibold text-gray-900 mb-3">
                    Gelişmiş Özellikler
                  </h4>
                  <div className="space-y-2">
                    {[
                      { label: 'Gelişmiş Analitik', value: plan.hasAdvancedAnalytics },
                      { label: 'SMS Bildirimleri', value: plan.hasSMSNotifications },
                      { label: 'Email Bildirimleri', value: plan.hasEmailNotifications },
                      { label: 'Özel Marka', value: plan.hasCustomBranding },
                      { label: 'API Erişimi', value: plan.hasAPIAccess },
                      { label: 'Çoklu Şube', value: plan.hasMultiLocation },
                      { label: 'Paket Yönetimi', value: plan.hasPackageManagement },
                      { label: 'Google Takvim', value: plan.hasGoogleCalendarIntegration },
                      { label: 'Ödeme Entegrasyonu', value: plan.hasPaymentIntegration },
                    ].map((item, index) => (
                      <div key={index} className="flex items-center text-sm">
                        {item.value ? (
                          <span className="text-green-500 mr-2"><CheckIcon /></span>
                        ) : (
                          <span className="text-gray-300 mr-2"><XMarkIcon /></span>
                        )}
                        <span className={item.value ? 'text-gray-900' : 'text-gray-400'}>
                          {item.label}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* FAQ Section */}
        <div className="mt-20 max-w-3xl mx-auto">
          <h2 className="text-3xl font-bold text-center text-gray-900 mb-8">
            Sıkça Sorulan Sorular
          </h2>
          <div className="space-y-6">
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="font-semibold text-gray-900 mb-2">
                Ücretsiz deneme sürümü nasıl çalışır?
              </h3>
              <p className="text-gray-600">
                14 gün boyunca seçtiğiniz planın tüm özelliklerini ücretsiz kullanabilirsiniz. 
                Kredi kartı bilgisi gerektirmez. Deneme süresi bitiminde iptal edebilir veya 
                aboneliğinize devam edebilirsiniz.
              </p>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="font-semibold text-gray-900 mb-2">
                Plan değişikliği yapabilir miyim?
              </h3>
              <p className="text-gray-600">
                Evet, istediğiniz zaman planınızı yükseltebilir veya düşürebilirsiniz. 
                Yükseltme durumunda kalan süre için prorata hesaplama yapılır.
              </p>
            </div>
            <div className="bg-white rounded-lg shadow p-6">
              <h3 className="font-semibold text-gray-900 mb-2">
                İptal politikanız nedir?
              </h3>
              <p className="text-gray-600">
                Aboneliğinizi istediğiniz zaman iptal edebilirsiniz. İptal sonrası mevcut 
                dönem sonuna kadar hizmetinizi kullanmaya devam edebilirsiniz.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PricingPage;
