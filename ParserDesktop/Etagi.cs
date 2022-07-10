using System;
using HtmlAgilityPack;
using RestSharp;

namespace ParserDesktop
{
    public class Etagi
    {
        private HtmlNode _node;

        public async void CreateAd()
        {
            string ticket = "32453223";
            string idObject = String.Empty;
            // var client = new RestClient("https://ries3.etagi.com/tickets/open_empty/?user=66819");
            // var request = new RestRequest("", Method.Post);
            // request.AddHeader("Accept", "*/*");
            // request.AddHeader("Cookie",
            //     "RIESSESSID=deu2tgajtbj275o9s6up9ftm76; SameSite=Strict; emuuid=c66aa07a-dd21-4587-9ad1-cdaefdaf918f; from_advertisement=false; ries-user-id=66819; selected_city=www; visit_source=www.google.com");
            // var response = await client.ExecuteAsync(request).ConfigureAwait(false);
            //
            // ticket = response.ResponseUri.AbsolutePath.Split("view/")[1].Split("/")[0];



            // var client = new RestClient("https://ries3.etagi.com/client/ajax/editThisContact/");
            // var request = new RestRequest("", Method.Post);
            // request.AddHeader("Cookie",
            //     "RIESSESSID=deu2tgajtbj275o9s6up9ftm76; SameSite=Strict; emuuid=c66aa07a-dd21-4587-9ad1-cdaefdaf918f; from_advertisement=false; ries-user-id=66819; selected_city=www; visit_source=www.google.com; selected_tickets=" + ticket);
            // request.AddParameter("form[first_name]", "Руслаfн");
            // request.AddParameter("form[contacts][email]", " ");
            // request.AddParameter("form[contacts][tel][]", "8-952-889-20-01");
            // request.AddParameter("form[contacts][country_phone_mask][]", 1);
            // request.AddParameter("form[viber]", "0");
            // request.AddParameter("form[whatsapp]", "0");
            // request.AddParameter("form[telegram]", "0");
            // request.AddParameter("form[subscribe_to_newsletter]", "not_set");
            // request.AddParameter("form[type]", "phys");
            // request.AddParameter("form[org_form]", "Не указано");
            // request.AddParameter("form[post_select]", "0");
            // request.AddParameter("form[loyality_program_id]", "0");
            // request.AddParameter("subtypeContract", "0");
            // request.AddParameter("form[city_id]", "251");
            // request.AddParameter("form[client_id]", "1237047154");
            // request.AddParameter("form[ticket_id]", "32453289");
            // var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            // var client = new RestClient("https://ries3.etagi.com/ajax/create_object_from_ticket/");
            // var request = new RestRequest("", Method.Post);
            // request.AddHeader("Cookie",
            //     "RIESSESSID=deu2tgajtbj275o9s6up9ftm76; SameSite=Strict; emuuid=c66aa07a-dd21-4587-9ad1-cdaefdaf918f; from_advertisement=false; ries-user-id=66819; selected_city=www; visit_source=www.google.com; selected_tickets=" + ticket);
            // request.AddParameter("ticket_id", ticket);
            // request.AddParameter("action", "sale");
            // request.AddParameter("class_object", "flats");
            // request.AddParameter("client_id", "1237047931");
            // request.AddParameter("form[viber]", "0");
            // request.AddParameter("user_id", "66819");
            // request.AddParameter("city_id", "251");
            // request.AddParameter("type_id", "3");
            // request.AddParameter("source", "Realtor");
            // request.AddParameter("realty_type", "flat");
            // request.AddParameter("device_source", "1");
            // var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            var client = new RestClient("https://ries3.etagi.com/ajax/load_penalty_info/");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Cookie",
                "RIESSESSID=deu2tgajtbj275o9s6up9ftm76; SameSite=Strict; emuuid=c66aa07a-dd21-4587-9ad1-cdaefdaf918f; from_advertisement=false; ries-user-id=66819; selected_city=www; visit_source=www.google.com; selected_tickets=" +
                ticket);
            request.AddParameter("ob[id]", idObject);
            request.AddParameter("ob[newhouses_id]", "0");
            request.AddParameter("ob[class]", "flats");
            request.AddParameter("ob[action]", "sale");
            request.AddParameter("ob[direction]", "supply");
            request.AddParameter("ob[status]", "prepared");
            request.AddParameter("ob[callstat_agents_id]", "null");
            request.AddParameter("ob[price]", ""); //data.price
            request.AddParameter("ob[suspected]", "no");
            request.AddParameter("ob[residence]", "null");
            request.AddParameter("ob[user_id]", "66819");
            request.AddParameter("ob[fee_type]", "ruble");
            request.AddParameter("ob[date_create]", "2022-05-03 21:24:21"); //???
            request.AddParameter("ob[date_update]", "2022-05-03 22:12:40"); //???
            request.AddParameter("ob[secondary]", "yes");
            request.AddParameter("ob[date_rise]", "2022-05-03 21:24:21"); //???
            request.AddParameter("ob[auction]", "no");
            request.AddParameter("ob[price_changed]", "no");
            request.AddParameter("ob[price_editor]", "null");
            request.AddParameter("ob[probable_price]", "null");
            request.AddParameter("ob[photoallowed]", "no");
            request.AddParameter("ob[city_id]", "251");
            request.AddParameter("ob[district_id]", "46230"); //идентификатор района ???
            request.AddParameter("ob[rooms_cnt]", "2"); //data.numberofRooms
            request.AddParameter("ob[area_total]", "332"); //data.totalArea
            request.AddParameter("ob[series_id]", "2484"); //Что это ???
            request.AddParameter("ob[sold_date]", "null");
            request.AddParameter("ob[sold_price]", "null");
            request.AddParameter("ob[future]", "1651595061.000000");
            request.AddParameter("ob[transfer_from]", "66819");
            request.AddParameter("ob[f_type]", "flat");
            request.AddParameter("ob[f_house_num]", "228"); //номер дома / буква
            request.AddParameter("ob[c_type]", "null");
            request.AddParameter("ob[c_house_num]", "null");
            request.AddParameter("ob[c_object_from]", "null");
            request.AddParameter("ob[o_type]", "null");
            request.AddParameter("ob[o_house_num]", "null");
            request.AddParameter("ob[quality_photo]", "null");
            request.AddParameter("ob[video_link]", "null");
            request.AddParameter("ob[docs_to_deals]", "0");
            request.AddParameter("ob[object_exchange]", "null");
            request.AddParameter("ob[type_exchange]", "null");
            request.AddParameter("ob[city_exchange]", "null");
            request.AddParameter("ob[district_exchange]", "null");
            request.AddParameter("ob[rooms_exchange]", "null");
            request.AddParameter("ob[price_exchange]", "null");
            request.AddParameter("ob[manager_id]", "37424");
            request.AddParameter("ob[total_share_ownership]", "no");
            request.AddParameter("ob[after_deposit]", "1");
            request.AddParameter("ob[displaying_on_pkk]", "null");
            request.AddParameter("ob[yard_area]", "0");
            request.AddParameter("ob[auction_sum]", "null");
            request.AddParameter("ob[auction_reason_types]", "null");
            request.AddParameter("ob[other_reason]", "null");
            request.AddParameter("ob[client_type]", "phys");
            request.AddParameter("ob[access_agent_status_active]", "false");
            request.AddParameter("ob[yarmarka_status]", "null");
            request.AddParameter("ob[has_reservation]", "false");
            request.AddParameter("ob[reservation_confirmed]", "false");
            request.AddParameter("ob[shared_type]", "0");
            request.AddParameter("ob[is_filial]", "0");
            request.AddParameter("ob[fields_updated_from_extract]", "false");
            request.AddParameter("ob[nh_category]", "null");
            request.AddParameter("ob[country_id]", "1");
            request.AddParameter("ob[related_cities][]", "403");
            request.AddParameter("ob[related_cities][]", "251");
            request.AddParameter("ob[realtor_city]", "251");
            request.AddParameter("ob[photos_cnt]", "0");

            var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine();
        }
    }
}