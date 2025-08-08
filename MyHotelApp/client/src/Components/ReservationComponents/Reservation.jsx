import avatarReservation from '../../assets/ReservationLogo.png';
import EntityList from '../EntityList';
import { useState } from 'react';
import axios from 'axios';
import { useEffect } from 'react'; // ako nisi već
export default function Reservation() {
  const [refresh, setRefresh] = useState(false);

  const handleEdit = (reservation) => {
    console.log('Edit:', reservation);
  };
  useEffect(() => {
      axios.get("/api/Reservation/GetAllReservations")
        .then(res => {
          console.log("Fetched reservations:", res.data);
        })
        .catch(err => {
          console.error("Error fetching reservations:", err);
        });
    }, []);
  const handleInfo = (reservation) => {
    const formatDate = (dateStr) => {
      const date = new Date(dateStr);
      return isNaN(date.getTime()) ? 'Unknown' : date.toLocaleDateString();
    };
    

    alert(
      `Room: ${reservation.room?.roomNumber || 'N/A'}\n` +
      `Guest: ${reservation.guest?.firstName || ''} ${reservation.guest?.lastName || ''} (${reservation.guest?.jmbg || 'N/A'})\n` +
      `From: ${formatDate(reservation.startDate)}\n` +
      `To: ${formatDate(reservation.endDate)}`
    );
  };

  const handleDelete = async (reservationID) => {
    if (!window.confirm(`Are you sure you want to delete this reservation?`)) return;

    try {
      await axios.delete(`/api/Reservation/DeleteReservation/${reservationID}`);
      alert('Reservation deleted successfully.');
      setRefresh(prev => !prev);
    } catch (err) {
      console.error('Error deleting reservation:', err);
      alert('Failed to delete reservation.');
    }
  };

  return (
    <EntityList
      addRoute="/addreservation"
      fetchUrl="/api/Reservation/GetAllReservations"
      backgroundImage={avatarReservation}
      renderFields={reservation => {
        const formatDate = (dateStr) => {
          const date = new Date(dateStr);
          return isNaN(date.getTime()) ? 'Unknown' : date.toLocaleDateString();
        };
        return (
          <>
            <p><strong>Room:</strong> {reservation.room?.roomNumber}</p>
            <p><strong>Guest:</strong> {reservation.guest?.fullName}</p>
            <p><strong>From:</strong> {formatDate(reservation.checkInDate)}</p>
            <p><strong>To:</strong> {formatDate(reservation.checkOutDate)}</p>
          </>
        );
      }}
      onEdit={handleEdit}
      onInfo={handleInfo}
      onDelete={handleDelete}
      idField="reservationID"
      refreshTrigger={refresh}
    />
  );
}
