import React from 'react';
import { Link } from 'react-router-dom';
import type { SalonDto } from '../services/salonService';

interface SalonCardProps {
  salon: SalonDto;
}

const SalonCard: React.FC<SalonCardProps> = ({ salon }) => {
  const placeholderImage = 'https://via.placeholder.com/400x300?text=Salon';

  return (
    <Link 
      to={`/salons/${salon.id}`}
      className="block bg-white rounded-lg shadow hover:shadow-lg transition-shadow duration-200"
    >
      <div className="relative h-48 overflow-hidden rounded-t-lg">
        <img
          src={salon.primaryImageUrl || placeholderImage}
          alt={salon.name}
          className="w-full h-full object-cover"
        />
        {salon.averageRating > 0 && (
          <div className="absolute top-2 right-2 bg-white px-2 py-1 rounded-full shadow flex items-center gap-1">
            <svg className="w-4 h-4 text-yellow-400 fill-current" viewBox="0 0 20 20">
              <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z" />
            </svg>
            <span className="text-sm font-semibold text-gray-900">
              {salon.averageRating.toFixed(1)}
            </span>
            <span className="text-xs text-gray-500">
              ({salon.reviewCount})
            </span>
          </div>
        )}
      </div>
      
      <div className="p-4">
        <h3 className="text-lg font-semibold text-gray-900 mb-1 line-clamp-1">
          {salon.name}
        </h3>
        
        {salon.description && (
          <p className="text-sm text-gray-600 mb-3 line-clamp-2">
            {salon.description}
          </p>
        )}
        
        <div className="flex items-center text-sm text-gray-500 mb-2">
          <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span className="line-clamp-1">{salon.address}, {salon.city}</span>
        </div>
        
        {salon.phone && (
          <div className="flex items-center text-sm text-gray-500">
            <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
            </svg>
            <span>{salon.phone}</span>
          </div>
        )}
      </div>
    </Link>
  );
};

export default SalonCard;
