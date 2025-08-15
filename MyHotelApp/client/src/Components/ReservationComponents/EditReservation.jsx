import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import {
  checkGuestExists,
  checkRoomExists,
  checkRoomAvailability,
  handleRoomServiceChange,
  handleExtraServiceChange
} from '../ReservationUtils';

export default function EditReservation(){
    const navigate = useNavigate();
    const { reservationID } = useParams();

    const [showRoomServices, setShowRoomServices] = useState(false);
    const [showExtraServices, setShowExtraServices] = useState(false);
    const [roomServices, setRoomServices] = useState([]);
    const [extraServices, setExtraServices] = useState([]);

    const [guestExists, setGuestExists] = useState([]);
    const [guestID, setGuestID] = useState('');
    const [roomExists, setRoomExists] = useState([]);
    const [roomAvailable, setRoomAvailable] = useState([]);
    const [totalPrice, setTotalPrice] = useState(0);
    const [errorMessages, setErrorMessages] = useState([]);

    const [formData, setFormData] = useState({
        reservationID: '',
        roomNumber: '',
        guestID: '',
        checkInDate: '',
        checkOutDate: '',
        totalPrice: '', 
        roomServiceIDs: [],
        extraServiceIDs: []
    });


    useEffect(() => {
        async function fetchReservation() {
            try{
                const res = await axios.get(`/api/Reservation/GetReservationById/${reservationID}`);
                const data = res.data;

                const formattedData = {
                    ...data,
                    checkInDate: data.checkInDate?.split('T')[0],   // "YYYY-MM-DD"
                    checkOutDate: data.checkOutDate?.split('T')[0],
                    roomServiceIDs: data.roomServices?.map(s => s.roomServiceID) || [],
                    extraServiceIDs: data.extraServices?.map(s => s.extraServiceID) || []
                };

                setFormData(formattedData);

                

            }
            catch(err){
                console.error(err);
                alert('Failed to load reservation data.');
            }
        }

        fetchReservation();
        axios.get('/api/RoomService/GetAllRoomServices')
        .then(res => setRoomServices(res.data))
        .catch(err => console.error('An error occurred loading Room Service', err));

        axios.get('/api/ExtraService/GetAllExtraServices')
        .then(res => setExtraServices(res.data))
        .catch(err => console.error('An error occurred loading Extra Service', err));

        
        if(formData.guestID) 
        checkGuestExists(formData.guestID);
        if(formData.roomNumber) 
        checkRoomExists(formData.roomNumber);
        if(formData.roomNumber && formData.checkInDate && formData.checkOutDate)
        checkRoomAvailability(formData.roomNumber, formData.checkInDate, formData.checkOutDate);

        
        
    }, [reservationID,
        formData.guestID,
        formData.checkInDate,
        formData.checkOutDate,
        formData.roomServiceIDs,
        formData.extraServiceIDs,
        formData.roomNumber]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
        ...prev,
        [name]: value
        }));
    };

    const handleDelete = async (reservationID) => {
        if (!window.confirm(`Are you sure you want to delete this reservation?`)) return;

        try {
        await axios.delete(`/api/Reservation/DeleteReservation/${reservationID}`);
        alert('Reservation deleted successfully.');
        navigate("/reservation");
        //setRefresh(prev => !prev);
        } catch (err) {
        console.error('Error deleting reservation:', err);
        alert('Failed to delete reservation.');
        }
    };

    const handleGuestCheck = async (guestID) => {
        await checkGuestExists(guestID, setGuestExists, setGuestID);
    };

    const handleRoomCheck = async (roomNumber) => {
        await checkRoomExists(roomNumber, setRoomExists);
    };

    const handleRoomAvailability = async () => {
        await checkRoomAvailability(formData, setRoomAvailable);
    };

    
    const handleSubmit = async (e) => {
        e.preventDefault();
        const errors = [];

        //SVE ZIVE PROVERE DODAJ -- ubacila sam iz add
        const checkIn = new Date(formData.checkInDate);
        const checkOut = new Date(formData.checkOutDate);

        if (checkOut <= checkIn) {
        setErrorMessages(["Check-out date must be after check-in date."]);
        return;
        }

        if(guestExists === false){
        setErrorMessages(["Guest with that JMBG doens't exist."]);
        return;
        }
        if(roomExists === false){
        setErrorMessages(["Room with that number doens't exist."]);
        return;
        }

        if(roomAvailable === false){
        setErrorMessages(["Room isn't available for selected dates."]);
        return;
        }

        if (errors.length > 0) {
            setErrorMessages(errors);
            return;
        }

        try{
            await axios.put(`/api/Reservation/UpdateReservation/${reservationID}`, {
                ...formData,
                totalPrice: totalPrice
            });
            alert('Reservation updated successfully!');
            navigate("/reservation");
        }
        catch(err){
            console.error(err);
            setErrorMessages([err.response?.data?.message || 'Update failed.']);
        }
    };

    useEffect(() => {
    const calculateTotal = async () => {
      
      if(!formData.checkInDate || !formData.checkOutDate || !formData.roomNumber)
        return;

      const days = (new Date(formData.checkOutDate) - new Date(formData.checkInDate)) / (1000 * 60 * 60 * 24); //jer je rez u ms

      if(days <= 0) 
        return;

      let total = 0;

      //cena sobe
      try {
        const roomRes = await axios.get(`/api/Room/GetRoom/${formData.roomNumber}`);
        total += roomRes.data.roomType.pricePerNight * days;//posle extra serv * dani pa sve to plus room serv
      }
      catch(err){
        console.error('Room price fetch failed:', err);
      }

      //room serv cena
      for(let id of formData.roomServiceIDs) {
        const service = roomServices.find(s => s.roomServiceID === id);
        if(service)
          total += parseFloat(service.itemPrice || 0);
      }

      // extra service cene
      for (let id of formData.extraServiceIDs) {
        const service = extraServices.find(s => s.extraServiceID === id);
        if (service) 
          total += parseFloat(service.price || 0) * days;
      }

      const roundedTotal = parseFloat(total.toFixed(2));
      if (roundedTotal !== totalPrice) {
        setTotalPrice(roundedTotal);
      }

    };

    calculateTotal();

  }, [
      formData.checkInDate,
      formData.checkOutDate,
      formData.roomNumber,
      formData.roomServiceIDs,
      formData.extraServiceIDs,
      roomServices,
      extraServices,
      totalPrice
  ]);

    const handleExit = () => {
        navigate("reservation");
    }

    const formatErrors = (errorsObj) => {
        let messages = [];
        for (const field in errorsObj) {
        const errors = errorsObj[field];
        messages.push(`${field}: ${errors.join(', ')}`);
        }
        return messages;
    };

    return (
        <form className="reservation-form" onSubmit={handleSubmit}>
            <button type="button" className="exit-button" onClick={handleExit}>
                x
            </button>
            <div className="form-group">
                <label className="form-label">Guest JMBG:</label>
                <input type="text"
                    name="guestID"
                    className="form-input"
                    value={formData.guestID}
                    onChange={handleChange}
                    onBlur={() => handleGuestCheck(formData.guestID, setGuestExists, setGuestID)}
                />
                {guestExists === false && (
                <span className="form-error">Guest with that JMBG doesn't exist.</span>
                )}
            </div>

            <div className="form-group">
                <label className="form-label">Room Number:</label>
                <input type="text"
                    name="roomNumber" 
                    className="form-input" 
                    value={formData.roomNumber}
                    onChange={handleChange}
                    onBlur={() => handleRoomCheck(formData.roomNumber, setRoomExists)} 
                />
                {roomExists === false && (
                <span className="form-error">Room with that number doesn't exist.</span>
                )}
            </div>

            <div className="form-group">
                <label className="form-label">Check-In Date:</label>
                <input type="date" 
                    name="checkInDate" 
                    className="form-input" 
                    value={formData.checkInDate}
                    onChange={handleChange}
                    onBlur={() => handleRoomAvailability(formData, setRoomAvailable)}
                />
                {roomAvailable === false && (
                <span className="form-error">Room is not available for selected dates.</span>
                )}
            </div>

            <div className="form-group">
                <label className="form-label">Check-Out Date:</label>
                <input type="date" 
                    name="checkOutDate" 
                    className="form-input" 
                    value={formData.checkOutDate}
                    onChange={handleChange}
                    onBlur={() => handleRoomAvailability(formData, setRoomAvailable)}
                />
                {roomAvailable === false && (
                <span className="form-error">Room is not available for selected dates.</span>
                )}
            </div>

            <div className="button-pair-row">
                <div className="toggle-group-inline">
                <button
                    type="button"
                    className="form-button"
                    onClick={() => setShowRoomServices(!showRoomServices)}
                >
                    Room Services
                </button>
                {showRoomServices && (
                    <div className="checkbox-group">
                    <label className="form-label">Room Services:</label>
                    {roomServices.map((service) => (
                        <label key={service.roomServiceID} className="checkbox-item">
                        <input 
                            type="checkbox" 
                            name="roomServices" 
                            value={service.roomServiceID}
                            checked={formData.roomServiceIDs.includes(service.roomServiceID)}
                            onChange={() => handleRoomServiceChange(service.roomServiceID, setFormData)} />
                        {service.itemName}
                        </label>
                    ))}
                    </div>
                )}
                </div>

                <div className="toggle-group-inline">
                <button
                    type="button"
                    className="form-button"
                    onClick={() => setShowExtraServices(!showExtraServices)}
                >
                    Extra Services
                </button>
                {showExtraServices && (
                    <div className="checkbox-group">
                    <label className="form-label">Extra Services:</label>
                    {extraServices.map((service) => (
                        <label key={service.extraServiceID} className="checkbox-item">
                        <input type="checkbox" name="extraServices" value={service.extraServiceID} onChange={() => handleExtraServiceChange(service.extraServiceID, setFormData)}/>
                        {service.serviceName}
                        </label>
                    ))}
                    </div>
                )}
                </div>
            </div>

            <div className="form-group">
                <label className="form-label">Total Price:</label>
                <div className="price-input-wrapper">
                <span className="currency-symbol">$</span>
                <input
                    type="text"
                    name="totalPrice"
                    className="form-input price-input"
                    value={formData.totalPrice}
                    onChange={handleChange}
                    readOnly
                />
                </div>
            </div>
            
            <button type="submit" className="form-button">Add reservation</button>
            <button 
                type="button" 
                className="form-button delete" 
                onClick={(e) => {
                e.preventDefault(); 
                handleDelete(reservationID);
                }}>
                Delete Reservation
            </button>


            {errorMessages.length > 0 && (
                <div className="error-messages" style={{ color: 'red', marginTop: '1rem' }}>
                <h4>Errors:</h4>
                <ul>
                    {errorMessages.map((msg, idx) => (
                    <li key={idx}>{msg}</li>
                    ))}
                </ul>
                </div>
            )}

        </form>
    );
}