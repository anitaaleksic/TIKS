//import avatarReservation from '../../assets/ReservationLogo.png';
//import EntityList from '../EntityList';
import { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import "../../css/Reservation.css";

export default function Reservation() {
  //const [refresh, setRefresh] = useState(false);
  const [reservations, setReservations] = useState([]);
  const navigate = useNavigate();

  // const handleEdit = (reservation) => {
  //   console.log('Edit:', reservation);
  //   navigate(`/editreservation/${reservation.reservationID}`);
  // };

  const handleEdit = (reservationID) => navigate(`/reservation/edit/${reservationID}`);

  const handleAdd = () => {
    navigate("/addreservation");
  }

  async function GetAllReservations() {
    const response = await axios.get("/api/Reservation/GetAllReservations");
    return response.data;
  }

  useEffect(() => {
      async function loadReservations() {
        const data = await GetAllReservations();
        setReservations(data);
      }
      loadReservations();
    }, []);

  // const handleInfo = (reservation) => {
  //   const formatDate = (dateStr) => {
  //     const date = new Date(dateStr);
  //     return isNaN(date.getTime()) ? 'Unknown' : date.toLocaleDateString();
  //   };
    

    // alert(
    //   `Room: ${reservation.room?.roomNumber || 'N/A'}\n` +
    //   `Guest: ${reservation.guest?.firstName || ''} ${reservation.guest?.lastName || ''} (${reservation.guest?.jmbg || 'N/A'})\n` +
    //   `From: ${formatDate(reservation.startDate)}\n` +
    //   `To: ${formatDate(reservation.endDate)}`
    // );
  //};

  

  const formatDate = (dateStr) => {
          const date = new Date(dateStr);
          return isNaN(date.getTime()) ? 'Unknown' : date.toLocaleDateString();
  };

  return (
    <div className="entity-page-wrapper">
      <div className="entity-header">
        <button onClick={handleAdd} className="form-button large">
          Add Reservation
        </button>
      </div>

      <div className='entity-grid'>
        {reservations.map((reservation) => (
          <div key={reservation.reservationID} className='entity-card' onClick={() => handleEdit(reservation.reservationID)} data-reservation-id={reservation.reservationID}> 
            <span className='res-header'>Reservation #{reservation.reservationID}</span>
            <span className='date'>{formatDate(reservation.checkInDate)} - {formatDate(reservation.checkOutDate)}</span>

            <span>
              <div>
              <div className='property'>
                Room: <span className="highlight">{reservation.room?.roomNumber}</span>
              </div>
              <div className='property'>
                Guest: <span className="highlight">{reservation.guest?.fullName}</span>
              </div>
              </div>
              <div className='property'>Room Services: 
                  {reservation.roomServices?.map((rs) => (
                    <div className='services'>
                        {rs.itemName}
                        <span className="dots"></span>
                        <span className="price">{rs.itemPrice.toFixed(2)}$</span>
                     </div>
                  ))}
              </div>
              <div className='property'>Extra Services: 
                  {reservation.extraServices?.map((es) => (
                      <div className='services'>
                          {es.serviceName}
                          <span className="dots"></span>
                          <span className="price">{es.price.toFixed(2)}$</span>
                      </div>  
                  ))}
              </div>
              <div className="price res-total">Total: {reservation.totalPrice}$</div>
            </span>



          </div>
          
      ))}
      </div>

      {/* <div className='entity-grid'>
        {reservations.map((reservation) => (
          <div key={reservation.reservationID} className='entity-card'> 
            <table className='reservation-table'>
              <thead>
                <h3>Reservation #{reservation.reservationID}</h3>
                <th className='th-reservation'>{formatDate(reservation.checkInDate)} - {formatDate(reservation.checkOutDate)}</th>
              </thead>
              <tbody className='reservation-body'>
                <tr>
                  <td>Room: {reservation.room?.roomNumber}</td>
                </tr>
                <tr>
                  Guest: {reservation.guest?.fullName}
                </tr>
                <tr>
                  Room Services: 
                  {reservation.roomServices?.map((rs) => (
                    <tr key={rs.roomServiceID}> 
                      <td className='td-reservation'>
                        {rs.itemName}
                        <span className="dots"></span>
                        <span className="price">{rs.itemPrice.toFixed(2)}$</span>
                      </td>
                    </tr>
                  ))}
                </tr>
                <tr>Extra Services: 
                  {reservation.roomServices?.map((rs) => (
                      <tr key={rs.roomServiceID}>
                        <td className='td-reservation'>
                          {rs.itemName}
                          <span className="dots"></span>
                          <span className="price">{rs.itemPrice.toFixed(2)}$</span>
                        </td>
                      </tr>
                    
                  ))}
                </tr>
                <tr>
                  <td className="price">
                    Total: {reservation.totalPrice}$
                  </td>
                </tr>
              
              </tbody>
              </table>
          </div>
        ))}

      </div> */}
    </div>
    
  );
}
