import './css/App.css';
import { Routes, Route } from 'react-router-dom';
import Navbar from './Components/Navbar';
import Guest from './Components/GuestComponents/Guest';       
import Room from './Components/RoomComponents/Room';  
import RoomService from './Components/RoomServiceComponents/RoomService';
import AddExtraService from './Components/ExtraServiceComponents/AddExtraService';
import AddGuest from './Components/GuestComponents/AddGuest';   
import AddRoom from './Components/RoomComponents/AddRoom';
import AddRoomService from './Components/RoomServiceComponents/AddRoomService';
import AddReservation from './Components/ReservationComponents/AddReservation';
import ExtraService from './Components/ExtraServiceComponents/ExtraService';
import Reservation from './Components/ReservationComponents/Reservation';
import EditGuest from './Components/GuestComponents/EditGuest';
import EditRoom from './Components/RoomComponents/EditRoom';
import EditExtraService from './Components/ExtraServiceComponents/EditExtraService';
import EditRoomService from './Components/RoomServiceComponents/EditRoomService';
import EditReservation from './Components/ReservationComponents/EditReservation';

function App() {
  return (
    <>
      <Navbar />
      <Routes>
        <Route path="/" element={<Reservation />} />
        <Route path="/guest" element={<Guest />} />        
        <Route path="/addguest" element={<AddGuest />} />  
        <Route path="/room" element={<Room />} />
        <Route path="/addroom" element={<AddRoom />} />
        <Route path="/roomservice" element={<RoomService />} />
        <Route path="/addroomservice" element={<AddRoomService />} />
        <Route path="/extraservice" element={<ExtraService />} />
        <Route path="/addextraservice" element={<AddExtraService />} />
        <Route path="/reservation" element={<Reservation />} />
        <Route path="/addreservation" element={<AddReservation />} />
        <Route path="/editguest/:jmbg" element={<EditGuest />} />
        <Route path="/room/edit/:roomNumber" element={<EditRoom />} />
        <Route path="/extraservice/edit/:serviceName" element={<EditExtraService />} />
        <Route path="/roomservice/edit/:itemName" element={<EditRoomService />} />
        <Route path="/reservation/edit/:reservationID" element={<EditReservation />} />
      </Routes>
    </>
  );
}

export default App;
